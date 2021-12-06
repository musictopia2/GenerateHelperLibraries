using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace GenerateHelperLibraries;
internal static class Extensions
{
    
    //do this way so if in comment, then still okay.
    public static bool DidIncludeCodeAtLeastOnce(this Compilation compilation)
    {
        return compilation.SyntaxTrees.Any(xx =>
        {
            string text = xx.ToString();
            text = text.Replace("//[IncludeCode", "");
            return text.Contains("[IncludeCode");
        });
    }
    public static bool DidIgnoreCode(this SyntaxTree syntax)
    {
        string text = syntax.ToString();
        text = text.Replace("//[IgnoreCode", "");
        return text.Contains("[IgnoreCode");
    }
    public static bool DidIncludeCode(this SyntaxTree syntax)
    {
        string text = syntax.ToString();
        text = text.Replace("//[IncludeCode", "");
        return text.Contains("[IncludeCode");
    }
    public static string GetFileNameForCopy(this SyntaxTree tree)
    {
        var temps = tree.GetCompilationUnitRoot();
        var nexts = temps.Members.OfType<FileScopedNamespaceDeclarationSyntax>().First().Name.ToString();
        string path = tree.FilePath;
        string fileName = Path.GetFileNameWithoutExtension(path);
        string finalName = $"{nexts}.{fileName}.g";
        return finalName;
    }
    public static string GetCSharpString(this string content)
    {
        content = content.Replace("\"", "\"\"");
        return content;
    }
    //fix to the removeattributeproblem
    public static string RemoveAttribute(this string content, string attributeName)
    {
        content = content.Replace($"    [{attributeName}]{Environment.NewLine}", "");
        content = content.Replace($"[{attributeName}]{Environment.NewLine}", "");
        return content;
    }
#pragma warning disable RS2008 // Enable analyzer release tracking
    private static DiagnosticDescriptor ReportError(this string errorMessage, string id) => new(id,
#pragma warning restore RS2008 // Enable analyzer release tracking
        "Could not create source generation",
        errorMessage,
        "Error",
        DiagnosticSeverity.Error,
        true
        );

    public static void ReportError(this GeneratorExecutionContext context, string errorMessgae, string id)
    {
        context.ReportDiagnostic(Diagnostic.Create(errorMessgae.ReportError(id), Location.None));
    }
}