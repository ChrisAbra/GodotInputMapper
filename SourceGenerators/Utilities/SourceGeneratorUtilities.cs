using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban;

namespace GodotInputMapper;

internal static class SourceGeneratorUtilities
{
    internal static string GenerateSource(string templateResourceName, object model)
    {

        var assembly = Assembly.GetExecutingAssembly();

        using (Stream stream = assembly.GetManifestResourceStream(templateResourceName))
        using (StreamReader reader = new StreamReader(stream))
        {
            string template = reader.ReadToEnd();

            var scribanTemplate = Template.Parse(template);
            try
            {
                return scribanTemplate.Render(model);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }


    internal static TAttribute MapToAttributeType<TAttribute>(AttributeData attributeData)
    {

        TAttribute attribute;

        if (attributeData.AttributeConstructor != null && attributeData.ConstructorArguments.Length > 0)
        {
            attribute = (TAttribute)Activator.CreateInstance(typeof(TAttribute), GetActualConstuctorParams(attributeData).ToArray());
        }
        else
        {
            attribute = (TAttribute)Activator.CreateInstance(typeof(TAttribute));
        }

        foreach (var p in attributeData.NamedArguments)
        {
            typeof(TAttribute).GetField(p.Key).SetValue(attribute, p.Value.Value);
        }

        return attribute;
    }

    internal static IEnumerable<object> GetActualConstuctorParams(AttributeData attributeData)
    {

        foreach (var arg in attributeData.ConstructorArguments)
        {
            if (arg.Kind == TypedConstantKind.Array)
            {
                // Assume they are strings, but the array that we get from this
                // should actually be of type of the objects within it, be it strings or ints
                // This is definitely possible with reflection, I just don't know how exactly. 
                yield return arg.Values.Select(a => a.Value).OfType<string>().ToArray();
            }
            else
            {
                #pragma warning disable CS8603 // disables null property warning;
                yield return arg.Value;
            }
        }
    }

    internal static string PrivateVariableToPublic(string? s)
    {
        if (s is null) return null;
        s = s.Replace("_", "");
        return char.ToUpper(s[0]) + s.Substring(1);

    }
    internal static string PublicVariableToPrivate(string? s)
    {
        if (s is null) return null;
        s = char.ToLower(s[0]) + s.Substring(1);
        return "_" + s;

    }

    internal static List<object> logs = new();
    internal static void Debug(SourceProductionContext spc)
    {
        var sb = new StringBuilder("public class Debug { public static string DebugOutput = @\"");
        sb.AppendLine("");
        foreach (var output in logs)
        {
            sb.AppendLine(output.ToString());
        }
        sb.AppendLine("\";}");

        spc.AddSource("Debug", sb.ToString());
    }


    internal static ClassDeclarationSyntax? GetClassDeclarationSyntax(SyntaxNode syntax)
    {
        if (syntax is ClassDeclarationSyntax classDec) return classDec;

        else
        {
            while (syntax.Parent is not null)
            {
                if (syntax.Parent is ClassDeclarationSyntax cds) return cds;
                else
                {
                    if (syntax.Parent is null) return null;
                    syntax = syntax.Parent;
                }
            }
            return null;

        }
    }
    internal static List<string> GetBaseImplementationFromClassDeclaration(ClassDeclarationSyntax cds)
    {

        List<string> bases = new();
        if (cds?.BaseList?.Types is null) return bases;

        foreach (var baseType in cds.BaseList.Types)
        {
            bases.Add(baseType.ToString());
        }

        return bases;
    }

    internal static string GetNamespaceFromClassDeclaration(ClassDeclarationSyntax cds)
    {

        string nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode? potentialNamespaceParent = cds.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
                potentialNamespaceParent is not NamespaceDeclarationSyntax
                && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we 
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        return nameSpace;

    }


}
