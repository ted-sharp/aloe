namespace Aloe.Apps.MedockLib.Services.VoiceCommand;

public class AzureOpenAiSettings
{
    public const string SectionName = "AzureOpenAI";
    public string? Endpoint { get; set; }
    public string? ApiKey { get; set; }
    public string? DeploymentName { get; set; }
    public bool IsConfigured =>
        !String.IsNullOrWhiteSpace(this.Endpoint) &&
        !String.IsNullOrWhiteSpace(this.ApiKey) &&
        !String.IsNullOrWhiteSpace(this.DeploymentName);
}
