using System.Text;
using System.Text.Json;
using API.Contacts.Model;
using API.Contacts.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace API.Contacts.Services;

/// <summary>
/// Implementation of the AI assistant service using Azure OpenAI
/// </summary>
public class AzureOpenAIAssistantService : IAiAssistantService
{
    private readonly HttpClient _httpClient;
    private readonly AiAssistantOptions _options;
    private readonly IDistributedCache _cache;
    private readonly IRateLimitingService _rateLimitingService;

    public AzureOpenAIAssistantService(
        HttpClient httpClient,
        IOptions<AiAssistantOptions> options,
        IDistributedCache cache,
        IRateLimitingService rateLimitingService)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
        _rateLimitingService = rateLimitingService;

        // Configure the HTTP client
        _httpClient.BaseAddress = new Uri(_options.Endpoint);
        _httpClient.DefaultRequestHeaders.Add("api-key", _options.ApiKey);
    }

    public async Task<string> GetAiResponseAsync(IEnumerable<Message> conversationHistory, string userId)
    {
        // Check rate limiting
        if (await IsRequestLimitReachedAsync(userId))
        {
            throw new InvalidOperationException("Request limit reached for this user");
        }

        // Convert messages to the format expected by Azure OpenAI
        var messages = ConvertToAiMessages(conversationHistory);

        // Prepare the request payload
        var requestObject = new
        {
            messages,
            model = _options.DeploymentName,
            max_tokens = _options.MaxTokens,
            temperature = _options.Temperature
        };

        var requestJson = JsonSerializer.Serialize(requestObject);
        var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        // Make the API call
        var response = await _httpClient.PostAsync(
            $"/openai/deployments/{_options.DeploymentName}/chat/completions?api-version={_options.ApiVersion}",
            requestContent);

        response.EnsureSuccessStatusCode();

        // Parse the response
        var responseBody = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);
        
        var aiResponse = responseObject
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        // Update rate limiting
        await RecordRequestAsync(userId);

        return aiResponse;
    }

    public async Task<string> RegenerateResponseAsync(IEnumerable<Message> conversationHistory, string messageId)
    {
        // Get the user ID from the last user message
        var lastUserMessage = conversationHistory
            .Where(m => !m.AuthorId.Equals("system", StringComparison.OrdinalIgnoreCase) && 
                        m.Id != messageId)
            .OrderByDescending(m => m.Timestamp)
            .FirstOrDefault();

        if (lastUserMessage == null)
        {
            throw new InvalidOperationException("No user message found to regenerate from");
        }

        // Filter out the message being regenerated and messages after it
        var regenerationTimestamp = conversationHistory
            .First(m => m.Id == messageId)
            .Timestamp;

        var filteredHistory = conversationHistory
            .Where(m => m.Timestamp < regenerationTimestamp)
            .ToList();

        // Get the new AI response
        return await GetAiResponseAsync(filteredHistory, lastUserMessage.AuthorId);
    }

    public async Task<bool> IsRequestLimitReachedAsync(string userId)
    {
        return !await _rateLimitingService.AllowRequestAsync(userId, "ai-chat");
    }

    private async Task RecordRequestAsync(string userId)
    {
        // Using the rate limiting service to record the request
        await _rateLimitingService.AllowRequestAsync(userId, "ai-chat");
    }

    private static List<object> ConvertToAiMessages(IEnumerable<Message> conversationHistory)
    {
        return conversationHistory
            .OrderBy(m => m.Timestamp)
            .Select(m => new
            {
                role = GetRole(m.AuthorId),
                content = m.Text
            })
            .ToList<object>();
    }

    private static string GetRole(string authorId)
    {
        return authorId.Equals("system", StringComparison.OrdinalIgnoreCase) 
            ? "system"
            : authorId.Contains("assistant", StringComparison.OrdinalIgnoreCase) 
                ? "assistant" 
                : "user";
    }
}
