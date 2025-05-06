using API.Contacts.Application.Interfaces;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace API.Contacts.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the file message handler for processing file attachments in messages
    /// </summary>
    public class FileMessageHandler : IFileMessageHandler
    {
        private readonly ILogger<FileMessageHandler> _logger;
        private readonly string _fileServerBaseUrl;

        // Regular expression to identify file messages
        // Format: [file:filename.ext](https://fileserver.com/files/uuid)
        private static readonly Regex FileMessageRegex = new Regex(
            @"\[file:(.*?)\]\((https?://.*?)\)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public FileMessageHandler(ILogger<FileMessageHandler> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileServerBaseUrl = configuration["FileServer:BaseUrl"] ?? "https://files.example.com";
        }

        /// <summary>
        /// Checks if a message contains a file attachment
        /// </summary>
        public bool IsFileMessage(string messageText)
        {
            if (string.IsNullOrEmpty(messageText))
            {
                return false;
            }

            return FileMessageRegex.IsMatch(messageText);
        }

        /// <summary>
        /// Extracts file information from a message
        /// </summary>
        public (string FileName, string FileUrl)? ExtractFileInfo(string messageText)
        {
            if (string.IsNullOrEmpty(messageText))
            {
                return null;
            }

            var match = FileMessageRegex.Match(messageText);
            if (!match.Success || match.Groups.Count < 3)
            {
                return null;
            }

            var fileName = match.Groups[1].Value;
            var fileUrl = match.Groups[2].Value;

            return (fileName, fileUrl);
        }

        /// <summary>
        /// Creates a file message text from file information
        /// </summary>
        public string CreateFileMessage(string fileName, string fileUrl)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (string.IsNullOrEmpty(fileUrl))
            {
                throw new ArgumentNullException(nameof(fileUrl));
            }

            // Create the formatted file message text
            return $"[file:{fileName}]({fileUrl})";
        }

        /// <summary>
        /// Validates a file URL
        /// </summary>
        public async Task<bool> ValidateFileUrlAsync(string fileUrl)
        {
            try
            {
                // Check if the URL belongs to our file server domain
                if (string.IsNullOrEmpty(fileUrl) || !fileUrl.StartsWith(_fileServerBaseUrl))
                {
                    _logger.LogWarning("Invalid file URL domain: {FileUrl}", fileUrl);
                    return false;
                }

                // In a real implementation, you might want to check if the file exists
                // by making a HEAD request to the URL or checking a database

                // For this example, we'll just return true for URLs with the correct domain
                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file URL: {FileUrl}", fileUrl);
                return false;
            }
        }

        /// <summary>
        /// Generates a file preview URL if available
        /// </summary>
        public async Task<string> GetFilePreviewUrlAsync(string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl) || !fileUrl.StartsWith(_fileServerBaseUrl))
                {
                    return null;
                }

                // Extract the file ID from the URL
                var fileId = ExtractFileIdFromUrl(fileUrl);
                if (string.IsNullOrEmpty(fileId))
                {
                    return null;
                }

                // Check if the file is an image or document that can be previewed
                if (await IsPreviewableFileAsync(fileId))
                {
                    // Generate a preview URL by appending a query parameter or path segment
                    return $"{fileUrl}/preview";
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating preview URL for file: {FileUrl}", fileUrl);
                return null;
            }
        }

        /// <summary>
        /// Extracts the file ID from a file URL
        /// </summary>
        private string ExtractFileIdFromUrl(string fileUrl)
        {
            try
            {
                var uri = new Uri(fileUrl);
                var segments = uri.Segments;

                // Assume the file ID is the last segment of the URL path
                if (segments.Length > 0)
                {
                    return segments[^1].TrimEnd('/');
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting file ID from URL: {FileUrl}", fileUrl);
                return null;
            }
        }

        /// <summary>
        /// Checks if a file can be previewed
        /// </summary>
        private async Task<bool> IsPreviewableFileAsync(string fileId)
        {
            // In a real implementation, this would check the file's metadata
            // (e.g., file extension, MIME type) to determine if it can be previewed

            // For this example, we'll just return true
            await Task.CompletedTask;
            return true;
        }
    }
}
