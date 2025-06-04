using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Contacts.Models;
using Common.Utils;
using DevExtreme.AspNet.Data.ResponseModel;

namespace API.Contacts.WebSocket;

/// <summary>
/// Defines the contract for the SignalR Chat Hub
/// This interface represents all the methods that clients can call on the server
/// Following Interface Segregation Principle (ISP) by grouping related operations
/// </summary>
public interface IChatHub
{
    #region Conversation Operations
    
    /// <summary>
    /// Loads conversations for the current user with DevExtreme load options support
    /// This method integrates with DevExtreme's DataSource to provide
    /// server-side filtering, sorting, and paging
    /// </summary>
    /// <param name="loadOptions">DevExtreme load options for data processing</param>
    /// <returns>LoadResult containing conversations and metadata</returns>
    Task<LoadResult> LoadConversation(DevExtremeLoadOptions loadOptions, string conversationType = "Contacts");
    
    /// <summary>
    /// Creates a new conversation (either direct or group)
    /// The method determines the type based on the number of participants
    /// </summary>
    /// <param name="title">Title for group conversations (null for direct)</param>
    /// <param name="participantIds">List of participant user IDs</param>
    /// <returns>The newly created conversation</returns>
    Task<Conversation> CreateConversation(string title, List<string> participantIds, string conversationType = "Contacts");
    
    /// <summary>
    /// Updates an existing conversation's properties
    /// Only certain fields can be updated (title, active status)
    /// </summary>
    /// <param name="conversationId">The conversation to update</param>
    /// <param name="updates">Dictionary of field names and new values</param>
    /// <returns>Task representing the async operation</returns>
    Task UpdateConversation(Guid conversationId, Dictionary<string, object> updates);
    
    /// <summary>
    /// Archives (soft deletes) a conversation
    /// The conversation remains in the database but is marked as inactive
    /// </summary>
    /// <param name="conversationId">The conversation to archive</param>
    /// <returns>Task representing the async operation</returns>
    Task ArchiveConversation(Guid conversationId);
    
    #endregion

    #region Message Operations
    
    /// <summary>
    /// Loads messages for a specific conversation with DevExtreme support
    /// Messages are loaded in reverse chronological order by default
    /// </summary>
    /// <param name="conversationId">The conversation to load messages from</param>
    /// <param name="loadOptions">DevExtreme load options for pagination</param>
    /// <returns>LoadResult containing messages and metadata</returns>
    Task<LoadResult> LoadMessages(Guid conversationId, DevExtremeLoadOptions loadOptions);
    
    /// <summary>
    /// Sends a new message to a conversation
    /// This will trigger real-time notifications to all participants
    /// </summary>
    /// <param name="conversationId">The target conversation</param>
    /// <param name="content">The message content</param>
    /// <param name="type">The type of message (default: Text)</param>
    /// <param name="metadata">Optional metadata for special message types</param>
    /// <param name="replyToMessageId">Optional ID of message being replied to</param>
    /// <returns>The sent message with server-generated properties</returns>
    Task<Message> SendMessage(
        Guid conversationId, 
        string content, 
        MessageType type = MessageType.Text,
        string metadata = null,
        Guid? replyToMessageId = null);
    
    /// <summary>
    /// Edits an existing message
    /// Only the original sender can edit their messages
    /// </summary>
    /// <param name="messageId">The message to edit</param>
    /// <param name="newContent">The new content</param>
    /// <returns>True if edit was successful, false otherwise</returns>
    Task<bool> EditMessage(Guid messageId, string newContent);
    
    /// <summary>
    /// Deletes a message (soft delete - content is replaced)
    /// Only the original sender can delete their messages
    /// </summary>
    /// <param name="messageId">The message to delete</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    Task<bool> DeleteMessage(Guid messageId);
    
    /// <summary>
    /// Marks a message as read by the current user
    /// Updates read receipts for all participants
    /// </summary>
    /// <param name="messageId">The message to mark as read</param>
    /// <returns>Task representing the async operation</returns>
    Task MarkMessageAsRead(Guid messageId);
    
    #endregion

    #region Group Management
    
    /// <summary>
    /// Adds participants to an existing group conversation
    /// Only group conversations support this operation
    /// </summary>
    /// <param name="conversationId">The group conversation</param>
    /// <param name="userIds">List of user IDs to add</param>
    /// <returns>Task representing the async operation</returns>
    Task AddParticipants(Guid conversationId, List<string> userIds);
    
    /// <summary>
    /// Removes a participant from a group conversation
    /// Users can remove themselves or admins can remove others
    /// </summary>
    /// <param name="conversationId">The group conversation</param>
    /// <param name="userId">The user to remove</param>
    /// <returns>Task representing the async operation</returns>
    Task RemoveParticipant(Guid conversationId, string userId);
    
    /// <summary>
    /// Allows a user to leave a group conversation
    /// This is a self-service version of RemoveParticipant
    /// </summary>
    /// <param name="conversationId">The conversation to leave</param>
    /// <returns>Task representing the async operation</returns>
    Task LeaveConversation(Guid conversationId);
    
    #endregion

    #region User Presence and Typing Indicators
    
    /// <summary>
    /// Notifies other participants that the user is typing
    /// This creates the "user is typing..." indicator in the UI
    /// </summary>
    /// <param name="conversationId">The conversation where user is typing</param>
    /// <returns>Task representing the async operation</returns>
    Task StartTyping(Guid conversationId);
    
    /// <summary>
    /// Notifies other participants that the user stopped typing
    /// Clears the typing indicator in the UI
    /// </summary>
    /// <param name="conversationId">The conversation where user stopped typing</param>
    /// <returns>Task representing the async operation</returns>
    Task StopTyping(Guid conversationId);
    
    #endregion

    #region Connection Management
    
    /// <summary>
    /// Called when a client connects to the hub
    /// Sets up user presence and loads initial data
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    Task OnConnectedAsync();
    
    /// <summary>
    /// Called when a client disconnects from the hub
    /// Cleans up user presence and typing indicators
    /// </summary>
    /// <param name="exception">Optional exception if disconnect was due to error</param>
    /// <returns>Task representing the async operation</returns>
    Task OnDisconnectedAsync(Exception exception);
    
    #endregion
}
