using API.Contacts.Application.Dtos;
using System.Threading.Tasks;

namespace API.Contacts.Application.Interfaces
{
    /// <summary>
    /// Interface for managing typing indicators
    /// </summary>
    public interface ITypingService
    {
        /// <summary>
        /// Records that a user has started typing in a conversation
        /// </summary>
        Task UserStartedTypingAsync(string conversationId, string userId);

        /// <summary>
        /// Records that a user has stopped typing in a conversation
        /// </summary>
        Task UserStoppedTypingAsync(string conversationId, string userId);

        /// <summary>
        /// Gets users who are currently typing in a conversation
        /// </summary>
        Task<IEnumerable<UserDto>> GetUsersTypingAsync(string conversationId, string excludeUserId = null);

        /// <summary>
        /// Checks if a user is currently typing in a conversation
        /// </summary>
        Task<bool> IsUserTypingAsync(string conversationId, string userId);
    }
}
