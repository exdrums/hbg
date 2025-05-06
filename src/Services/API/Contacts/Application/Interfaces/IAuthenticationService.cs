using System.Threading.Tasks;

namespace API.Contacts.Application.Interfaces
{
    /// <summary>
    /// Interface for authentication services
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Checks if a user is authorized to access a conversation
        /// </summary>
        Task<bool> IsUserAuthorizedForConversationAsync(string userId, string conversationId);

        /// <summary>
        /// Checks if a user is authorized to add participants to a conversation
        /// </summary>
        Task<bool> CanAddParticipantsAsync(string userId, string conversationId);

        /// <summary>
        /// Checks if a user is authorized to remove participants from a conversation
        /// </summary>
        Task<bool> CanRemoveParticipantsAsync(string userId, string conversationId, string participantToRemoveId);

        /// <summary>
        /// Checks if a user is authorized to update a conversation's properties
        /// </summary>
        Task<bool> CanUpdateConversationAsync(string userId, string conversationId);

        /// <summary>
        /// Gets user information from an OIDC token
        /// </summary>
        Task<(string OidcSubject, string DisplayName)> GetUserFromOidcTokenAsync(string token);

        /// <summary>
        /// Validates an API key
        /// </summary>
        Task<bool> ValidateApiKeyAsync(string apiKey);
    }
}
