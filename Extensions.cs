namespace GenerateHelperLibraries;
internal static class Extensions
{

    public static void ProcessIgnoreCode(this ResultsModel result)
    {
        string text = result.CompleteText;
        text = text.Replace("//[IgnoreCode", "");
        result.IgnoreCode = text.Contains("[IgnoreCode");
    }
    public static void ProcessIncludeCode(this ResultsModel result)
    {
        string text = result.CompleteText;
        text = text.Replace("//[IncludeCode", "");

        result.IncludeCode = text.Contains("[IncludeCode");
    }
}