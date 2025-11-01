namespace GenerateHelperLibraries;
internal record ResultsModel
{
    //not necessarily class name now.
    public string MainName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public string CompleteText { get; set; } = "";
    public bool IncludeCode { get; set; }
    public bool IgnoreCode { get; set; }
    public string FriendlyName()
    {
        string temps = Namespace.Replace(".", "");
        return $"{temps}{MainName}";
    }
}