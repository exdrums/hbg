namespace API.Contacts.Services;

/// <summary>
/// Configuration options for the AI assistant service
/// </summary>
public class AiAssistantOptions
{
    public string Endpoint { get; set; }
    public string ApiKey { get; set; }
    public string DeploymentName { get; set; }
    public int MaxTokens { get; set; } = 1000;
    public double Temperature { get; set; } = 0.7;
    public string ApiVersion { get; set; } = "2024-02-01";
    public int RequestLimitPerMinute { get; set; } = 5;
    public int RequestLimitCooldownMinutes { get; set; } = 1;
}
