using API.Contacts.Model;

namespace API.Contacts.Services.Interfaces;

/// <summary>
/// Interface for handling file attachment messages in chat conversations.
/// </summary>
public interface IFileMessageHandler
{
    /// <summary>
    /// Creates a file message in a conversation.
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="senderId">Sender user ID</param>
    /// <param name="fileName">File name</param>
    /// <param name="fileUrl">File URL</param>
    /// <returns>Created message entity</returns>
    Task<Message> CreateFileMessageAsync(string conversationId, string senderId, string fileName, string fileUrl);
    
    /// <summary>
    /// Extracts file information from a message text.
    /// </summary>
    /// <param name="messageText">Message text</param>
    /// <returns>Tuple containing (fileName, fileUrl) if message contains file info, null otherwise</returns>
    (string FileName, string FileUrl)? ExtractFileInfo(string messageText);
    
    /// <summary>
    /// Checks if a message contains file information.
    /// </summary>
    /// <param name="messageText">Message text</param>
    /// <returns>True if the message contains file info, false otherwise</returns>
    bool IsFileMessage(string messageText);
    
    /// <summary>
    /// Gets the download URL for a file.
    /// </summary>
    /// <param name="fileId">File ID</param>
    /// <returns>Download URL</returns>
    string GetFileDownloadUrl(string fileId);
}