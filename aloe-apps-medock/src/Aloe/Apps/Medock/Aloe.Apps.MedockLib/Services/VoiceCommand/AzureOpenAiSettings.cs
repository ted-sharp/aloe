namespace Aloe.Apps.MedockLib.Services.VoiceCommand;

public class AzureOpenAiSettings
{
    public const string SectionName = "AzureOpenAI";
    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }
    public string? DeploymentName { get; set; }
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(this.Endpoint) &&
        !string.IsNullOrWhiteSpace(this.ApiKey) &&
        !string.IsNullOrWhiteSpace(this.DeploymentName);
}
