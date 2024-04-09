using Microsoft.CodeAnalysis;
using System;
using System.Text;
namespace GenerateHelperLibraries;
[Generator]
public class MySourceGenerator : ISourceGenerator
{
    private void AppendToBuilder(ref StringBuilder builder, SyntaxTree tree)
    {
        string text = tree.ToString();
        string fileName = tree.GetFileNameForCopy();
        string spaces = "        ";
        string content = text.GetCSharpString();
        content = content.RemoveAttribute("IncludeCode");
        content = content.RemoveAttribute("IgnoreCode");
        string others = "#nullable enable";
        if (content.StartsWith(others) == false)
        {
            content = $"{others}{Environment.NewLine}{content}";
        }
        builder.AppendLine($@"{spaces}text = @""{content}"";");
        builder.AppendLine($@"{spaces}context.AddSource(""{fileName}"", text);");
    }
    public void Execute(GeneratorExecutionContext context)
    {
        Compilation compilation = context.Compilation;
        bool includeglobal = compilation.DidIncludeCodeAtLeastOnce();
        StringBuilder builder = new();
        builder.AppendLine("global using SourceCodeHelpers.Utilities;");
        builder.AppendLine("using Microsoft.CodeAnalysis;");
        builder.AppendLine("namespace SourceCodeHelpers.Utilities;");
        builder.AppendLine("internal static class BuilderExtensions");
        builder.AppendLine("{");
        builder.AppendLine("    internal static void BuildSourceCode(this IAddSource context)"); //a breaking change.  so this can be supported from incremental source generators which means increasing version (because of breaking change)
        builder.AppendLine("    {");
        builder.AppendLine("        string text;");
        bool hadOne = false;
        foreach (var item in compilation.SyntaxTrees)
        {

            if (item.ToString().Contains("namespace ") == false)
            {
                continue; //because there was none.
            }
            if (item.ToString().Contains("IIncrementalGenerator"))
            {
                continue; //you cannot generate source code for itself obviously //has to be t
            }
            if (item.ToString().Contains("ISourceGenerator"))
            {
                continue; //2 situations.  this means you can now support the new iincrementalgenerator.
            }
            if (item.ToString().Contains("ISyntaxReceiver"))
            {
                continue; //you cannot generate source code for syntax receivers
            }
            bool includSingle = item.DidIncludeCode();
            bool ignoreSingle = item.DidIgnoreCode();
            if (ignoreSingle && includSingle)
            {
                string error = "Cannot include and ignore code at the same time";
                //not reporting errors (has to test that next).
                context.ReportError(error, "IgnoreIncludeConflict");
                return;
            }
            if (item.DidIgnoreCode() && includeglobal == true)
            {
                string error = "Unable to generate source code because you ignored code even though you manually marked some to include";
                context.ReportError(error, "IgnoreIncludeConflict");
                return;
            }
            if (ignoreSingle)
            {
                continue; //because its being ignored.
            }
            if (includeglobal == true && includSingle == false)
            {
                continue;
            }
            hadOne = true;
            AppendToBuilder(ref builder, item);
        }
        if (hadOne == false)
        {
            return;
        }
        builder.AppendLine("    }");
        builder.AppendLine("}");
        string results = builder.ToString();
        context.AddSource("importlibrary.g", results);
    }
    public void Initialize(GeneratorInitializationContext context)
    {
        
    }
}