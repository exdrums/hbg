using System.Threading.Tasks;

namespace API.Constructor.Services
{
    public interface IFilesServiceClient
    {
        Task<UploadFileResult> UploadImageAsync(byte[] imageBytes, string fileName, string projectId);
        Task<string> GetImageUrlAsync(string fileId);
        Task<bool> DeleteImageAsync(string fileId);
    }

    public class UploadFileResult
    {
        public string FileId { get; set; }
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
