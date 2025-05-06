using System.Threading.Tasks;

namespace API.Contacts.Application.Interfaces
{
    /// <summary>
    /// Interface for handling file messages
    /// </summary>
    public interface IFileMessageHandler
    {
        /// <summary>
        /// Checks if a message contains a file attachment
        /// </summary>
        bool IsFileMessage(string messageText);

        /// <summary>
        /// Extracts file information from a message
        /// </summary>
        (string FileName, string FileUrl)? ExtractFileInfo(string messageText);

        /// <summary>
        /// Creates a file message text from file information
        /// </summary>
        string CreateFileMessage(string fileName, string fileUrl);

        /// <summary>
        /// Validates a file URL
        /// </summary>
        Task<bool> ValidateFileUrlAsync(string fileUrl);

        /// <summary>
        /// Generates a file preview URL if available
        /// </summary>
        Task<string> GetFilePreviewUrlAsync(string fileUrl);
    }
}
