using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GenerateHelperLibraries;
internal class EmitClass(ImmutableArray<ResultsModel> results, SourceProductionContext context)
{
    private static readonly DiagnosticDescriptor GlobalIncludeConflict = new(
    id: "GH002",
    title: "IncludeCode Global Conflict",
    messageFormat: "{0}",
    category: "GenerateHelperLibraries",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true
);
    public void Emit()
    {
       

        // ====================================================
        // 2. Global mode detection
        // ====================================================

        bool anyIncludes = results.Any(r => r.IncludeCode);
        bool anyIgnores = results.Any(r => r.IgnoreCode);

        // ====================================================
        // 3. Global conflict rule
        // ====================================================

        // If any include exists:
        // → ONLY include is allowed
        // → no ignore
        if (anyIncludes)
        {
            if (anyIgnores)
            {
                // GLOBAL CONFLICT
                context.ReportDiagnostic(Diagnostic.Create(
                    GlobalIncludeConflict,
                    Location.None,
                    "When any type uses [IncludeCode], all other types must also use [IncludeCode]. " +
                    "Types without IncludeCode or with IgnoreCode are not allowed."
                ));

                return;
            }
        }

        // ====================================================
        // 4. Apply correct filtering
        // ====================================================

        IEnumerable<ResultsModel> filtered;

        if (anyIncludes)
        {
            // strict include-only mode
            filtered = results.Where(r => r.IncludeCode);
        }
        else
        {
            // normal mode
            filtered = results.Where(r => r.IgnoreCode == false);
        }

        // ====================================================
        // 5. Emit code
        // ====================================================
        var finalList = filtered.ToImmutableList();


        foreach (var item in finalList)
        {
            WriteItem(item);
        }

        Globals(finalList);
    }
    private void Globals(IEnumerable<ResultsModel> list)
    {
        StringBuilder builder = new();
        builder.AppendLine("global using SourceCodeHelpers.Utilities;");
        builder.AppendLine("using Microsoft.CodeAnalysis;");
        builder.AppendLine("namespace SourceCodeHelpers.Utilities;");
        builder.AppendLine("internal static partial class BuilderExtensions");
        builder.AppendLine("{");
        builder.AppendLine("    internal static void BuildSourceCode(this IAddSource context)");
        string spaces = "        ";
        builder.AppendLine("    {");
        builder.AppendLine($"{spaces}string text;");
        foreach (var item in list)
        {
            string fileName = item.FriendlyName();
            builder.AppendLine($"{spaces}text = {fileName}();");
            builder.AppendLine($@"{spaces}context.AddSource(""{fileName}.g"", text);");
        }
        builder.AppendLine("    }");
        builder.AppendLine("}");
        string endings = builder.ToString();
        context.AddSource("importlibrary.g", endings);
    }
    private void WriteItem(ResultsModel item)
    {
        //probably can't do custom.
        //for now, just do part 1 (later can do part 2).
        StringBuilder builder = new();
        builder.AppendLine("namespace SourceCodeHelpers.Utilities;");
        builder.AppendLine("internal static partial class BuilderExtensions");
        builder.AppendLine("{");
        builder.AppendLine($"    private static string {item.FriendlyName()}()");
        builder.AppendLine("    {");
        builder.Append("        return ");
        builder.AppendLine(""""
            """
            """");
        builder.AppendLine(item.CompleteText);
        builder.AppendLine(""""
            """;
            """");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        string results = builder.ToString();
        context.AddSource($"{item.Namespace}{item.MainName}.g.cs", results);
    }
}