using API.Contacts.Models;

namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Defines the contract for conversation-related business logic
/// 
/// This service embodies the Repository pattern combined with
/// Domain Service pattern from Domain-Driven Design (DDD):
/// 
/// 1. Repository Pattern: Abstracts data access, making it easy to
///    switch between databases (SQL, NoSQL, in-memory for testing)
/// 
/// 2. Domain Service: Contains business logic that doesn't naturally
///    fit within a single entity (like checking user permissions)
/// 
/// The interface follows Interface Segregation Principle (ISP) by
/// focusing only on conversation operations, not mixing concerns
/// </summary>
public interface IConversationService
{
    #region Query Operations
    
    /// <summary>
    /// Retrieves all conversations for a specific user
    /// This includes both direct messages and group conversations
    /// 
    /// Implementation considerations:
    /// - Should filter out archived conversations unless requested
    /// - Should order by last message timestamp (most recent first)
    /// - Should include unread count for each conversation
    /// - Should be optimized for frequent calls (caching strategy)
    /// </summary>
    /// <param name="userId">The user whose conversations to retrieve</param>
    /// <param name="includeArchived">Whether to include archived conversations</param>
    /// <returns>Queryable collection of conversations for flexible filtering</returns>
    Task<IQueryable<Conversation>> GetUserConversationsAsync(string userId, bool includeArchived = false);
    
    /// <summary>
    /// Retrieves a single conversation by ID
    /// Used when loading a specific chat
    /// 
    /// Implementation should:
    /// - Include participant information for display
    /// - Return null if conversation doesn't exist
    /// - NOT check permissions (that's a separate concern)
    /// </summary>
    /// <param name="conversationId">The conversation to retrieve</param>
    /// <returns>The conversation or null if not found</returns>
    Task<Conversation> GetConversationAsync(Guid conversationId);
    
    /// <summary>
    /// Checks if a user has access to a specific conversation
    /// This is a critical security check used throughout the system
    /// 
    /// Access rules:
    /// - User must be a participant in the conversation
    /// - Conversation must be active (not deleted)
    /// - Future: Could check group permissions, blocked users, etc.
    /// </summary>
    /// <param name="userId">The user to check</param>
    /// <param name="conversationId">The conversation to check access for</param>
    /// <returns>True if user has access, false otherwise</returns>
    Task<bool> UserHasAccessAsync(string userId, Guid conversationId);
    
    /// <summary>
    /// Finds an existing direct conversation between two users
    /// Prevents duplicate direct conversations
    /// 
    /// Direct conversation rules:
    /// - Only one direct conversation should exist between two users
    /// - Should work regardless of parameter order (symmetric)
    /// - Should only return active conversations
    /// </summary>
    /// <param name="userId1">First user</param>
    /// <param name="userId2">Second user</param>
    /// <returns>Existing conversation or null</returns>
    Task<Conversation> FindDirectConversationAsync(string userId1, string userId2);
    
    #endregion

    #region Command Operations
    
    /// <summary>
    /// Creates a new direct conversation between two users
    /// Should check for existing conversations first
    /// 
    /// Business rules:
    /// - Cannot create conversation with yourself
    /// - Should reuse existing direct conversation if one exists
    /// - Should create initial read status for both users
    /// </summary>
    /// <param name="creatorUserId">The user initiating the conversation</param>
    /// <param name="otherUserId">The other participant</param>
    /// <returns>The created (or existing) conversation</returns>
    Task<Conversation> CreateDirectConversationAsync(string creatorUserId, string otherUserId);
    
    /// <summary>
    /// Creates a new group conversation
    /// Groups require a title and multiple participants
    /// 
    /// Business rules:
    /// - Must have at least 2 participants
    /// - Must have a non-empty title
    /// - Creator is automatically added as participant
    /// - All participants get default permissions
    /// </summary>
    /// <param name="creatorUserId">The user creating the group</param>
    /// <param name="title">The group title</param>
    /// <param name="participantIds">All participants including creator</param>
    /// <returns>The created group conversation</returns>
    Task<Conversation> CreateGroupConversationAsync(
        string creatorUserId, 
        string title, 
        IEnumerable<string> participantIds);
    
    /// <summary>
    /// Updates conversation properties
    /// Only certain fields should be updateable
    /// 
    /// Updateable fields:
    /// - Title (for groups only)
    /// - IsActive (for archiving)
    /// 
    /// Non-updateable fields:
    /// - ConversationId, Type, CreatedAt, etc.
    /// </summary>
    /// <param name="conversationId">The conversation to update</param>
    /// <param name="updates">Dictionary of field names and values</param>
    /// <returns>The updated conversation</returns>
    Task<Conversation> UpdateConversationAsync(Guid conversationId, Dictionary<string, object> updates);
    
    /// <summary>
    /// Archives (soft deletes) a conversation
    /// The conversation remains in the database but is hidden
    /// 
    /// Archive behavior:
    /// - Sets IsActive to false
    /// - Removes from active conversation lists
    /// - Preserves all messages and history
    /// - Can be reversed (unarchive)
    /// </summary>
    /// <param name="conversationId">The conversation to archive</param>
    Task ArchiveConversationAsync(Guid conversationId);
    
    /// <summary>
    /// Updates the last message information for a conversation
    /// Called whenever a new message is sent
    /// 
    /// This method:
    /// - Updates LastMessageAt timestamp
    /// - Updates LastMessagePreview for list display
    /// - Increments unread counts for other participants
    /// - Triggers re-sorting of conversation lists
    /// </summary>
    /// <param name="conversationId">The conversation that received a message</param>
    /// <param name="message">The new message</param>
    Task UpdateLastMessageAsync(Guid conversationId, Message message);
    
    #endregion

    #region Participant Management
    
    /// <summary>
    /// Adds a participant to a group conversation
    /// Only works for group conversations
    /// 
    /// Business rules:
    /// - Cannot add to direct conversations
    /// - Cannot add duplicate participants
    /// - Should create initial read status
    /// - Should send system message about addition
    /// </summary>
    /// <param name="conversationId">The group conversation</param>
    /// <param name="userId">The user to add</param>
    /// <returns>True if added, false if already present</returns>
    Task<bool> AddParticipantAsync(Guid conversationId, string userId);
    
    /// <summary>
    /// Removes a participant from a group conversation
    /// Enforces minimum participant requirements
    /// 
    /// Business rules:
    /// - Cannot remove from direct conversations
    /// - Must maintain at least 2 participants
    /// - Should clean up user's read status
    /// - Should send system message about removal
    /// </summary>
    /// <param name="conversationId">The group conversation</param>
    /// <param name="userId">The user to remove</param>
    /// <returns>True if removed, false otherwise</returns>
    Task<bool> RemoveParticipantAsync(Guid conversationId, string userId);
    
    /// <summary>
    /// Gets all participants of a conversation
    /// Useful for displaying member lists and permissions
    /// </summary>
    /// <param name="conversationId">The conversation</param>
    /// <returns>List of participant user IDs</returns>
    Task<IEnumerable<string>> GetParticipantsAsync(Guid conversationId);
    
    #endregion
}
