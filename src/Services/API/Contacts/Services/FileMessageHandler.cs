using System.Text.RegularExpressions;
using API.Contacts.Dtos;
using API.Contacts.Model;
using API.Contacts.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API.Contacts.Services;

/// <summary>
/// Handles file attachment messages in chat conversations.
/// </summary>
public class FileMessageHandler : IFileMessageHandler
{
    private readonly IMessageService _messageService;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<FileMessageHandler> _logger;
    
    // Regex pattern to extract file information from a message
    private static readonly Regex FilePattern = new Regex(@"\[File:\s*(.*?)\]\((.*?)\)", RegexOptions.Compiled);

    public FileMessageHandler(
        IMessageService messageService,
        IHubContext<ChatHub> hubContext,
        ILogger<FileMessageHandler> logger)
    {
        _messageService = messageService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Creates a file message in a conversation.
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    /// <param name="senderId">Sender user ID</param>
    /// <param name="fileName">File name</param>
    /// <param name="fileUrl">File URL</param>
    /// <returns>Created message entity</returns>
    public async Task<Message> CreateFileMessageAsync(string conversationId, string senderId, string fileName, string fileUrl)
    {
        try
        {
            // Format message text with markdown link
            string messageText = $"[File: {fileName}]({fileUrl})";
            
            // Create the message
            var message = await _messageService.CreateMessageAsync(conversationId, senderId, messageText);
            
            _logger.LogInformation("Created file message for file {FileName} in conversation {ConversationId}", 
                fileName, conversationId);
            
            return message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating file message in conversation {ConversationId}", conversationId);
            throw;
        }
    }

    /// <summary>
    /// Extracts file information from a message text.
    /// </summary>
    /// <param name="messageText">Message text</param>
    /// <returns>Tuple containing (fileName, fileUrl) if message contains file info, null otherwise</returns>
    public (string FileName, string FileUrl)? ExtractFileInfo(string messageText)
    {
        if (string.IsNullOrEmpty(messageText))
        {
            return null;
        }
        
        var match = FilePattern.Match(messageText);
        if (!match.Success || match.Groups.Count < 3)
        {
            return null;
        }
        
        return (match.Groups[1].Value, match.Groups[2].Value);
    }

    /// <summary>
    /// Checks if a message contains file information.
    /// </summary>
    /// <param name="messageText">Message text</param>
    /// <returns>True if the message contains file info, false otherwise</returns>
    public bool IsFileMessage(string messageText)
    {
        return !string.IsNullOrEmpty(messageText) && FilePattern.IsMatch(messageText);
    }

    /// <summary>
    /// Gets the download URL for a file.
    /// </summary>
    /// <param name="fileId">File ID</param>
    /// <returns>Download URL</returns>
    public string GetFileDownloadUrl(string fileId)
    {
        // In a real implementation, this would generate a secure URL to download the file
        // For this sample, we'll return a placeholder URL
        return $"/api/files/{fileId}";
    }
}