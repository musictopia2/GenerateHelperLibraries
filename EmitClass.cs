namespace GenerateHelperLibraries;
internal class EmitClass(ImmutableArray<ResultsModel> results, SourceProductionContext context)
{
    public void Emit()
    {
        foreach (var item in results)
        {
            WriteItem(item);
        }
        Globals();
    }
    private void Globals()
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
        foreach (var item in results)
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