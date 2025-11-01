using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Text;
namespace GenerateHelperLibraries;
[Generator] //this is important so it knows this class is a generator which will generate code for a class using it.
public class MySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declares1 = context.SyntaxProvider.CreateSyntaxProvider(
            (s, _) => s is BaseTypeDeclarationSyntax,
            (ctx, _) => (BaseTypeDeclarationSyntax)ctx.Node)
        .Where(n => n is not null)!;

        // Combine with compilation
        var declares2 = context.CompilationProvider.Combine(declares1.Collect());

        var declares3 = declares2.SelectMany(static (pair, _) =>
        {
            Compilation compilation = pair.Left;
            ImmutableArray<BaseTypeDeclarationSyntax> declarations = pair.Right;
            //found out i did not even need the compilation now.
            // Group by syntax tree (one per file)
            var grouped = declarations
                .GroupBy(d => d.SyntaxTree)
                .Select(g => g.OrderBy(d => d.SpanStart).First()); // first declaration in file

            // Convert to hashset for GetResults
            var start = ImmutableHashSet.CreateRange(grouped);
            return GetResults(start);
        });

        var declares4 = declares3.Collect();
        context.RegisterSourceOutput(declares4, Execute);
    }
    //decided to not worry if its partial.  since if it needs to be partial, will get immediate compile errors anyways.
    private bool IsSyntaxTarget(SyntaxNode syntax)
    {
        if (syntax is BaseTypeDeclarationSyntax)
        {
            return true;
        }
        return false;
        //return syntax is ClassDeclarationSyntax ctx &&
        //    ctx.IsPublic();
    }
    private BaseTypeDeclarationSyntax? GetTarget(GeneratorSyntaxContext context)
    {
        //until i can figure out something else.
        return (BaseTypeDeclarationSyntax)context.Node;
        //var ourClass = context.GetClassNode(); //can use the sematic model at this stage
        //var symbol = context.GetClassSymbol(ourClass);
        //return ourClass; //decided to not do the extras anymore.
    }
    private static ImmutableHashSet<ResultsModel> GetResults(
        ImmutableHashSet<BaseTypeDeclarationSyntax> nodes
        )
    {
        var builder = ImmutableHashSet.CreateBuilder<ResultsModel>();

        foreach (var node in nodes)
        {



            var result = new ResultsModel();



            // Try to extract namespace
            var ns = GetNamespace(node);
            result.Namespace = ns ?? "";

            // Extract the name (works for class, record, enum, struct)
            result.MainName = node.Identifier.Text;

            // Full text of the file or node
            // If you want the entire file’s text:
            result.CompleteText = node.SyntaxTree.GetText().ToString();

            if (result.CompleteText.Contains("IIncrementalGenerator"))
            {
                continue;
            }
            if (result.CompleteText.Contains("ISourceGenerator")) //just in case i have some old stuff.
            {
                continue;
            }
            if (result.CompleteText.Contains("ISyntaxReceiver"))
            {
                continue;
            }
            result.ProcessIgnoreCode();
            result.ProcessIncludeCode();

            string others = "#nullable enable";
            if (result.CompleteText.StartsWith(others) == false)
            {
                StringBuilder temps = new();
                temps.AppendLine(others);
                temps.Append(result.CompleteText);
                result.CompleteText = temps.ToString();
            }



            // If you only want this node’s text:
            // result.CompleteText = typeDecl.ToFullString();

            builder.Add(result);

        }
        return builder.ToImmutable();
    }

    /// <summary>
    /// Walks up the syntax tree to find the containing namespace.
    /// </summary>
    private static string? GetNamespace(SyntaxNode node)
    {
        SyntaxNode? current = node;
        while (current != null)
        {
            if (current is BaseNamespaceDeclarationSyntax ns)
            {
                var name = ns.Name.ToString();

                // Support nested namespaces (namespace A.B.C)
                var parentNs = GetNamespace(ns.Parent!);
                if (parentNs != null)
                {
                    return parentNs + "." + name;
                }

                return name;
            }
            current = current.Parent;
        }

        return null;
    }

    private void Execute(SourceProductionContext context, ImmutableArray<ResultsModel> list)
    {
        EmitClass emit = new(list, context);
        emit.Emit(); //start out with console.  later do reals once ready.
    }
}