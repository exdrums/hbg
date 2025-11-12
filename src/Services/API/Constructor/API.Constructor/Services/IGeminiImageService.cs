using System.Threading.Tasks;

namespace API.Constructor.Services
{
    public interface IGeminiImageService
    {
        Task<GeneratedImageResult> GenerateImageAsync(string prompt, string aspectRatio = "1:1");
        Task<byte[]> GenerateImageBytesAsync(string prompt, string aspectRatio = "1:1");
    }

    public class GeneratedImageResult
    {
        public byte[] ImageBytes { get; set; }
        public string Prompt { get; set; }
        public string AspectRatio { get; set; }
        public System.DateTime GeneratedAt { get; set; }
    }
}
