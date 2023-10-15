using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodotInputMapper;

[Generator]
public class InputMapperSourceGenerator : AttributeSourceGenerator<MapToInputAttribute>
{
    readonly string _templateResourceName = "GodotInputMapper.InputMapTemplate.sbncs";

    protected override List<Type> MatchingSyntaxNodeTypes => new List<Type>(){
            typeof(VariableDeclaratorSyntax),
            typeof(PropertyDeclarationSyntax),
            typeof(DelegateDeclarationSyntax)
        };

    public record struct PartialClassModel
    {
        public string Namespace;
        public string ClassName;
        public List<string> BaseClassList;
        public string BaseClassListText { get => string.Join(",", BaseClassList); }

        public record struct Member
        {
            public string MemberName;
            public string Type;

            public bool IsDelegate;
            public string DelegateEventName;
            public MapToInputAttribute Attribute;
        }

        public List<Member> GetterMembers;
        public List<Member> UnhandledInputMembers;
        public List<Member> InputMembers;

    }


    protected override void Execute(Compilation compilation, ImmutableArray<RelevantSyntax?> relevantSyntaxes, SourceProductionContext spc)
    {

        if (relevantSyntaxes.Count() == 0) return;

        var classesToGenerate = MapToModel(relevantSyntaxes);

        foreach (var classPair in classesToGenerate)
        {
            string partialClassSource = SourceGeneratorUtilities.GenerateSource(_templateResourceName, classPair.Value);
            spc.AddSource(classPair.Key, partialClassSource);
        }
    }

    protected Dictionary<string, PartialClassModel> MapToModel(ImmutableArray<RelevantSyntax?> relevantSyntaxes)
    {

        var classesToGenerate = new Dictionary<string, PartialClassModel>();

        foreach (var syntax in relevantSyntaxes)
        {

            if (syntax is not RelevantSyntax rs) continue;

            string className = rs.Class.Identifier.ToString();
            string fileName = $"{rs.Namespace}.{className}.g.cs";

            PartialClassModel.Member member = new()
            {
                Attribute = rs.Attribute
            };


            if (!classesToGenerate.TryGetValue(fileName, out PartialClassModel model))
            {
                model = new()
                {
                    Namespace = rs.Namespace,
                    ClassName = className,
                    BaseClassList = rs.BaseListSyntax,
                    GetterMembers = new(),
                    UnhandledInputMembers = new(),
                    InputMembers = new(),
                };
            }

            SourceGeneratorUtilities.logs.Add(model.GetterMembers.Count());

            member = rs.Syntax switch
            {
                VariableDeclaratorSyntax variable => member with
                {
                    MemberName = variable.Identifier.ToString(),
                    #pragma warning disable CS8601
                    Type = (variable.Parent is VariableDeclarationSyntax vds) ? vds.Type.ToString() : member.Type
                },
                PropertyDeclarationSyntax prop => member with
                {
                    MemberName = prop.Identifier.ToString(),
                    Type = prop.Type.ToString()
                },
                DelegateDeclarationSyntax del => member with
                {
                    MemberName = del.Identifier.ToString(),
                    IsDelegate = true,
                    DelegateEventName = del.Identifier.ToString().Replace("EventHandler", "")
                },
                _ => member
            };

            member = member.Attribute.Level switch
            {
                MapToInputAttribute.InputLevel.Getter => member with
                {
                    MemberName = SourceGeneratorUtilities.PublicVariableToPrivate(member.MemberName)
                },
                _ => member
            };


            switch (member.Attribute.Level)
            {
                case MapToInputAttribute.InputLevel.Getter:
                    {
                        model.GetterMembers.Add(member);
                        break;
                    }
                case MapToInputAttribute.InputLevel._UnhandledInput:
                    {
                        model.UnhandledInputMembers.Add(member);
                        break;
                    }
                case MapToInputAttribute.InputLevel._Input:
                    {
                        model.InputMembers.Add(member);
                        break;
                    }
                default: break;
            }

            classesToGenerate[fileName] = model;

        }

        return classesToGenerate;

    }

}
