using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using API.Files.Services;

namespace API.Files.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly IImageStorageService _imageStorage;
        private readonly AuthService _authService;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(
            IImageStorageService imageStorage,
            AuthService authService,
            ILogger<ImagesController> logger)
        {
            _imageStorage = imageStorage;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Upload a new image
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromForm] string projectId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!Array.Exists(allowedTypes, type => type == file.ContentType.ToLower()))
            {
                return BadRequest("Invalid file type. Only JPEG, PNG, and GIF images are allowed.");
            }

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest("File size exceeds 10MB limit");
            }

            try
            {
                var userId = _authService.GetUserId();

                // Read file bytes
                byte[] imageBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }

                // Store image
                var result = await _imageStorage.StoreImageAsync(
                    imageBytes,
                    file.FileName,
                    projectId,
                    userId
                );

                _logger.LogInformation("Image uploaded successfully. FileId: {FileId}, Size: {Size}",
                    result.FileId, result.FileSize);

                return Ok(new
                {
                    fileId = result.FileId,
                    url = result.Url,
                    thumbnailUrl = result.ThumbnailUrl,
                    fileSize = result.FileSize
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return StatusCode(500, "An error occurred while uploading the image");
            }
        }

        /// <summary>
        /// Get an image by ID
        /// </summary>
        [HttpGet("{fileId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImage(string fileId)
        {
            try
            {
                var image = await _imageStorage.GetImageAsync(fileId);

                if (image == null)
                {
                    return NotFound();
                }

                return File(image.Bytes, image.ContentType, image.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving image: {FileId}", fileId);
                return StatusCode(500, "An error occurred while retrieving the image");
            }
        }

        /// <summary>
        /// Get a thumbnail by ID
        /// </summary>
        [HttpGet("{fileId}/thumbnail")]
        [AllowAnonymous]
        public async Task<IActionResult> GetThumbnail(string fileId)
        {
            try
            {
                var thumbnail = await _imageStorage.GetThumbnailAsync(fileId);

                if (thumbnail == null)
                {
                    return NotFound();
                }

                return File(thumbnail.Bytes, thumbnail.ContentType, thumbnail.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving thumbnail: {FileId}", fileId);
                return StatusCode(500, "An error occurred while retrieving the thumbnail");
            }
        }

        /// <summary>
        /// Delete an image
        /// </summary>
        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteImage(string fileId)
        {
            try
            {
                var userId = _authService.GetUserId();
                var deleted = await _imageStorage.DeleteImageAsync(fileId, userId);

                if (!deleted)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {FileId}", fileId);
                return StatusCode(500, "An error occurred while deleting the image");
            }
        }

        /// <summary>
        /// Get image metadata
        /// </summary>
        [HttpGet("{fileId}/info")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImageInfo(string fileId)
        {
            try
            {
                var image = await _imageStorage.GetImageAsync(fileId);

                if (image == null)
                {
                    return NotFound();
                }

                return Ok(new
                {
                    fileId = fileId,
                    fileName = image.FileName,
                    contentType = image.ContentType,
                    size = image.Bytes.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving image info: {FileId}", fileId);
                return StatusCode(500, "An error occurred while retrieving image information");
            }
        }
    }
}
