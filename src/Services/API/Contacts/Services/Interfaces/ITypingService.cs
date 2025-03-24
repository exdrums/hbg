using API.Contacts.Model;

namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Service for handling typing indicators
/// </summary>
public interface ITypingService
{
    Task UserStartedTypingAsync(string conversationId, string userId);
    Task UserStoppedTypingAsync(string conversationId, string userId);
    Task<IEnumerable<User>> GetUsersTypingAsync(string conversationId, string excludeUserId);
}
