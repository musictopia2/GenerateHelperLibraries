using System;
using System.Linq;

namespace GenerateHelperLibraries;
internal static class Extensions
{
    public static void ProcessIgnoreCode(this ResultsModel result)
    {
        string text = result.CompleteText;

        // Detect the marker
        result.IgnoreCode = text.Contains("[IgnoreCode]");

        // Remove the entire line containing the marker
        var lines = text
            .Split(["\r\n", "\n", "\r"], StringSplitOptions.None)
            .Where(l => !l.Contains("[IgnoreCode]"));

        result.CompleteText = string.Join("\n", lines);
    }

    public static void ProcessIncludeCode(this ResultsModel result)
    {
        string text = result.CompleteText;

        result.IncludeCode = text.Contains("[IncludeCode]");

        var lines = text
            .Split(["\r\n", "\n", "\r"], StringSplitOptions.None)
            .Where(l => !l.Contains("[IncludeCode]"));

        result.CompleteText = string.Join("\n", lines);
    }
}