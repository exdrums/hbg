using API.Contacts.Domain.Models;
using System.Threading.Tasks;

namespace API.Contacts.Application.Interfaces
{
    /// <summary>
    /// Interface for AI assistant services
    /// </summary>
    public interface IAiAssistantService
    {
        /// <summary>
        /// Gets an AI response based on conversation history
        /// </summary>
        Task<string> GetAiResponseAsync(IEnumerable<Message> conversationHistory, string userId);

        /// <summary>
        /// Regenerates an AI response for a specific message
        /// </summary>
        Task<string> RegenerateResponseAsync(IEnumerable<Message> conversationHistory, string messageId);

        /// <summary>
        /// Checks if a user has reached their request limit
        /// </summary>
        Task<bool> IsRequestLimitReachedAsync(string userId);

        /// <summary>
        /// Gets the system prompt for the AI assistant
        /// </summary>
        Task<string> GetSystemPromptAsync();

        /// <summary>
        /// Creates an AI assistant user
        /// </summary>
        Task<User> CreateAiAssistantUserAsync();

        /// <summary>
        /// Gets information about API usage for a user
        /// </summary>
        Task<(int Used, int Limit)> GetApiUsageInfoAsync(string userId);
    }
}
