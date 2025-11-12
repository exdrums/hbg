using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace API.Files.Services
{
    public class ImageStorageService : IImageStorageService
    {
        private readonly ILogger<ImageStorageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _baseStoragePath;
        private readonly string _baseUrl;
        private const int ThumbnailSize = 300;

        public ImageStorageService(ILogger<ImageStorageService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            _baseStoragePath = _configuration["ImageStorage:BasePath"] ?? "./storage/images";
            _baseUrl = _configuration["HBGFILES"] ?? "http://localhost:5701";

            // Ensure base storage directory exists
            if (!Directory.Exists(_baseStoragePath))
            {
                Directory.CreateDirectory(_baseStoragePath);
                _logger.LogInformation("Created base storage directory: {Path}", _baseStoragePath);
            }
        }

        public async Task<ImageUploadResult> StoreImageAsync(byte[] imageBytes, string fileName, string projectId, string userId)
        {
            try
            {
                var fileId = Guid.NewGuid().ToString();
                var extension = Path.GetExtension(fileName).ToLower();
                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".jpg";
                }

                // Create user and project directories
                var userDir = Path.Combine(_baseStoragePath, "constructor", userId, projectId);
                var thumbnailDir = Path.Combine(userDir, "thumbnails");

                Directory.CreateDirectory(userDir);
                Directory.CreateDirectory(thumbnailDir);

                // File paths
                var imageFileName = $"{fileId}{extension}";
                var imagePath = Path.Combine(userDir, imageFileName);
                var thumbnailPath = Path.Combine(thumbnailDir, imageFileName);

                // Save original image
                await File.WriteAllBytesAsync(imagePath, imageBytes);
                _logger.LogInformation("Saved image: {Path}", imagePath);

                // Generate and save thumbnail
                await GenerateThumbnailAsync(imageBytes, thumbnailPath);
                _logger.LogInformation("Generated thumbnail: {Path}", thumbnailPath);

                // Build URLs
                var imageUrl = $"{_baseUrl}/api/images/{fileId}";
                var thumbnailUrl = $"{_baseUrl}/api/images/{fileId}/thumbnail";

                return new ImageUploadResult
                {
                    FileId = fileId,
                    Url = imageUrl,
                    ThumbnailUrl = thumbnailUrl,
                    FileSize = imageBytes.Length
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing image: {FileName}", fileName);
                throw;
            }
        }

        public async Task<ImageFileResult> GetImageAsync(string fileId)
        {
            try
            {
                var filePath = FindImageFile(fileId);
                if (filePath == null)
                {
                    _logger.LogWarning("Image not found: {FileId}", fileId);
                    return null;
                }

                var bytes = await File.ReadAllBytesAsync(filePath);
                var extension = Path.GetExtension(filePath).ToLower();
                var contentType = GetContentType(extension);

                return new ImageFileResult
                {
                    Bytes = bytes,
                    ContentType = contentType,
                    FileName = Path.GetFileName(filePath)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving image: {FileId}", fileId);
                throw;
            }
        }

        public async Task<ImageFileResult> GetThumbnailAsync(string fileId)
        {
            try
            {
                var thumbnailPath = FindThumbnailFile(fileId);
                if (thumbnailPath == null)
                {
                    _logger.LogWarning("Thumbnail not found: {FileId}", fileId);
                    return null;
                }

                var bytes = await File.ReadAllBytesAsync(thumbnailPath);
                var extension = Path.GetExtension(thumbnailPath).ToLower();
                var contentType = GetContentType(extension);

                return new ImageFileResult
                {
                    Bytes = bytes,
                    ContentType = contentType,
                    FileName = Path.GetFileName(thumbnailPath)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving thumbnail: {FileId}", fileId);
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string fileId, string userId)
        {
            try
            {
                var filePath = FindImageFile(fileId);
                if (filePath == null)
                {
                    return false;
                }

                // Verify user owns the file
                if (!filePath.Contains(userId))
                {
                    _logger.LogWarning("User {UserId} attempted to delete file not owned by them: {FileId}", userId, fileId);
                    return false;
                }

                // Delete image
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Delete thumbnail
                var thumbnailPath = FindThumbnailFile(fileId);
                if (thumbnailPath != null && File.Exists(thumbnailPath))
                {
                    File.Delete(thumbnailPath);
                }

                _logger.LogInformation("Deleted image and thumbnail: {FileId}", fileId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {FileId}", fileId);
                return false;
            }
        }

        private async Task GenerateThumbnailAsync(byte[] imageBytes, string outputPath)
        {
            using var image = Image.Load(imageBytes);

            // Calculate thumbnail dimensions maintaining aspect ratio
            var ratio = (double)ThumbnailSize / Math.Max(image.Width, image.Height);
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            image.Mutate(x => x.Resize(newWidth, newHeight));

            await image.SaveAsync(outputPath, new JpegEncoder { Quality = 85 });
        }

        private string FindImageFile(string fileId)
        {
            var constructorPath = Path.Combine(_baseStoragePath, "constructor");
            if (!Directory.Exists(constructorPath))
            {
                return null;
            }

            var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            foreach (var ext in extensions)
            {
                var fileName = $"{fileId}{ext}";
                var files = Directory.GetFiles(constructorPath, fileName, SearchOption.AllDirectories);

                // Exclude thumbnails
                foreach (var file in files)
                {
                    if (!file.Contains("thumbnails"))
                    {
                        return file;
                    }
                }
            }

            return null;
        }

        private string FindThumbnailFile(string fileId)
        {
            var constructorPath = Path.Combine(_baseStoragePath, "constructor");
            if (!Directory.Exists(constructorPath))
            {
                return null;
            }

            var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            foreach (var ext in extensions)
            {
                var fileName = $"{fileId}{ext}";
                var files = Directory.GetFiles(constructorPath, fileName, SearchOption.AllDirectories);

                // Only include thumbnails
                foreach (var file in files)
                {
                    if (file.Contains("thumbnails"))
                    {
                        return file;
                    }
                }
            }

            return null;
        }

        private string GetContentType(string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
    }
}
