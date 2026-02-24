namespace Aloe.Apps.RazorReportLib.Models;

public class PapersConfiguration
{
    public List<PaperDefinition> Papers { get; set; } = new();
    public string DefaultPaper { get; set; } = "A4";
    public string DefaultOrientation { get; set; } = "Portrait";

    public PaperDefinition? GetPaper(string name)
    {
        return Papers.FirstOrDefault(p => p.Name == name);
    }

    public Orientation GetDefaultOrientation()
    {
        return Enum.Parse<Orientation>(DefaultOrientation);
    }
}
