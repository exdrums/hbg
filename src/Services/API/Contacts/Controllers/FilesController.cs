using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using API.Contacts.Services.Interfaces;

namespace API.Contacts.Controllers;

/// <summary>
/// Controller for handling file uploads and downloads for chat conversations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileMessageHandler _fileMessageHandler;
    private readonly IOidcAuthenticationService _authService;
    private readonly ILogger<FilesController> _logger;
    private readonly FileExtensionContentTypeProvider _contentTypeProvider;
    
    // Maximum file size (5 MB by default, configurable)
    private readonly long MaxFileSize;
    
    // Allowed file extensions (for security, configurable)
    private readonly string[] AllowedExtensions;
    
    // File storage path (would be configured from settings in a real app)
    private readonly string _storageBasePath;

    public FilesController(
        IFileMessageHandler fileMessageHandler,
        IOidcAuthenticationService authService,
        IConfiguration configuration,
        ILogger<FilesController> logger)
    {
        _fileMessageHandler = fileMessageHandler;
        _authService = authService;
        _logger = logger;
        _contentTypeProvider = new FileExtensionContentTypeProvider();
        
        // Get storage path from configuration or use a default path
        _storageBasePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Path.GetTempPath(), "ChatFiles");
        
        // Get maximum file size from configuration or use default (5 MB)
        var maxSizeMB = configuration.GetValue<int>("FileStorage:MaxFileSizeMB", 5);
        MaxFileSize = maxSizeMB * 1024 * 1024;
        
        // Get allowed extensions from configuration or use default
        var configExtensions = configuration.GetSection("FileStorage:AllowedExtensions").Get<string[]>();
        AllowedExtensions = configExtensions ?? new[]
        {
            ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", 
            ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv", ".zip"
        };
        
        // Ensure storage directory exists
        if (!Directory.Exists(_storageBasePath))
        {
            Directory.CreateDirectory(_storageBasePath);
        }
    }

    /// <summary>
    /// Uploads a file for a specific conversation.
    /// </summary>
    /// <param name="conversationId">Conversation ID</param>
    [HttpPost("upload/{conversationId}")]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> UploadFile(string conversationId, [FromForm] IFormFile file)
    {
        try
        {
            // Check if file is provided
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded");
            }
            
            // Check file size
            if (file.Length > MaxFileSize)
            {
                return BadRequest($"File size exceeds the maximum allowed size of {MaxFileSize / (1024 * 1024)} MB");
            }
            
            // Check file extension
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(fileExtension))
            {
                return BadRequest($"File type {fileExtension} is not allowed");
            }
            
            // Get the current user ID
            var oidcSubject = User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(oidcSubject))
            {
                return Unauthorized("User not authenticated");
            }
            
            var userId = await _authService.GetUserIdFromOidcSubjectAsync(oidcSubject);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }
            
            // Verify user has access to conversation
            var isAuthorized = await _authService.IsUserAuthorizedForConversationAsync(userId, conversationId);
            if (!isAuthorized)
            {
                return Forbid("User is not authorized to access this conversation");
            }
            
            // Generate unique filename to prevent collisions
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_storageBasePath, uniqueFileName);
            
            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            // Generate file URL
            var fileUrl = _fileMessageHandler.GetFileDownloadUrl(uniqueFileName);
            
            // Create file message in the conversation
            var message = await _fileMessageHandler.CreateFileMessageAsync(
                conversationId, 
                userId, 
                file.FileName, 
                fileUrl
            );
            
            _logger.LogInformation("File {FileName} uploaded to conversation {ConversationId} by user {UserId}", 
                file.FileName, conversationId, userId);
            
            // Return the URL for the uploaded file
            return Ok(new { 
                fileUrl,
                messageId = message.Id,
                fileName = file.FileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to conversation {ConversationId}", conversationId);
            return StatusCode(500, "Error uploading file");
        }
    }

    /// <summary>
    /// Downloads a file.
    /// </summary>
    /// <param name="fileId">File ID (unique filename)</param>
    [HttpGet("{fileId}")]
    [AllowAnonymous] // Allow anonymous access to files (but add proper authorization checks inside)
    public async Task<IActionResult> DownloadFile(string fileId)
    {
        try
        {
            // Verify file ID format and prevent path traversal
            if (string.IsNullOrEmpty(fileId) || fileId.Contains("..") || fileId.Contains('/') || fileId.Contains('\\'))
            {
                return BadRequest("Invalid file ID");
            }
            
            // Construct file path
            var filePath = Path.Combine(_storageBasePath, fileId);
            
            // Check if file exists
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found");
            }
            
            // Determine content type
            if (!_contentTypeProvider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            
            // Get file stream
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            
            // Return file
            return new FileStreamResult(fileStream, contentType)
            {
                EnableRangeProcessing = true,
                FileDownloadName = Path.GetFileName(filePath)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileId}", fileId);
            return StatusCode(500, "Error downloading file");
        }
    }
}