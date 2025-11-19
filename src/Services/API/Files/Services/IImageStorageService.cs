using System.Threading.Tasks;

namespace API.Files.Services
{
    public interface IImageStorageService
    {
        Task<ImageUploadResult> StoreImageAsync(byte[] imageBytes, string fileName, string projectId, string userId);
        Task<ImageFileResult> GetImageAsync(string fileId);
        Task<bool> DeleteImageAsync(string fileId, string userId);
        Task<ImageFileResult> GetThumbnailAsync(string fileId);
    }

    public class ImageUploadResult
    {
        public string FileId { get; set; }
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
        public long FileSize { get; set; }
    }

    public class ImageFileResult
    {
        public byte[] Bytes { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }
}
