using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GodotInputMapper;



public abstract class AttributeSourceGenerator<TAttribute> : IIncrementalGenerator
where TAttribute : Attribute
{
    public record struct RelevantSyntax
    {
        public string Namespace;
        public List<string> BaseListSyntax;
        public ClassDeclarationSyntax Class;
        public SyntaxNode Syntax;
        public TAttribute Attribute;
    }


    protected abstract List<Type> MatchingSyntaxNodeTypes { get; }
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
                               .ForAttributeWithMetadataName(typeof(TAttribute).FullName,
                                                             (node, _) => checkPredicate(node),
                                                             GetSemanticTargetForGeneration)
                                                             .Where(m => m is not null);

        var compilation = context.CompilationProvider.Combine(provider.Collect());
        context.RegisterSourceOutput(compilation,
            (spc, source) =>
            {
                Execute(source.Left, source.Right, spc);
                SourceGeneratorUtilities.Debug(spc);
            });

    }

    private bool checkPredicate(SyntaxNode node)
    {
        return MatchingSyntaxNodeTypes.Contains(node.GetType());
    }


    protected abstract void Execute(Compilation compilation, ImmutableArray<RelevantSyntax?> relevantSyntaxes, SourceProductionContext spc);

    public virtual RelevantSyntax? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context, CancellationToken ct)
    {

        var attribute = context.Attributes.First();

        if (context.TargetNode is not SyntaxNode syntax) return null;

        var cds = SourceGeneratorUtilities.GetClassDeclarationSyntax(context.TargetNode);

        if (cds is null) return null;


        RelevantSyntax rs = new()
        {
            Class = cds,
            Namespace = SourceGeneratorUtilities.GetNamespaceFromClassDeclaration(cds),
            BaseListSyntax = SourceGeneratorUtilities.GetBaseImplementationFromClassDeclaration(cds),
            Syntax = syntax,
            Attribute = SourceGeneratorUtilities.MapToAttributeType<TAttribute>(attribute)
        };

        return rs;

    }
}

