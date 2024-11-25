using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using API.Files.Model;
using API.Files.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Files.Controllers;

[Route("files/projects/{projectID}/planimage/{planID}")]
[ApiController]
[Authorize]
public class PlansController : Controller
{
    private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "HBG_FILES");
    private readonly ProjectPermissionsService _permissionService;

    public PlansController(ProjectPermissionsService permissionService)
    {
        _permissionService = permissionService;

        // Ensure the storage directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    
    // Create (Upload)
    [HttpPost]
    public async Task<IActionResult> UploadImage([FromQuery] IFormFile file, int projectID, int planID)
    {
        var userId = GetUserId(); // Replace with actual user ID retrieval logic

        // Check permissions
        if (!await _permissionService.CanAccessProject(userId, projectID))
        {
            return Forbid();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("Invalid file.");
        }

        // Save file
        var image = new ProjectPlanImage
        {
            Id = planID,
            FileName = $"planimage{Path.GetExtension(file.FileName)}",
            OriginalName = file.FileName,
            FileType = file.ContentType,
            FileSize = file.Length,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = userId,
            ProjectId = projectID
        };

        await image.SaveToFileSystemAsync(file.OpenReadStream());

        return Ok(image);
    }

    // Read (Get Image File)
    [HttpGet]
    public async Task<IActionResult> GetImage(int projectID, int planID)
    {
        var userId = GetUserId();

        // Check permissions
        if (!await _permissionService.CanAccessProject(userId, projectID))
        {
            return Forbid();
        }

        // TODO: Fetch image metadata from the database
        var image = new ProjectPlanImage
        {
            Id = planID,
            FileName = "planimage.jpg", // Placeholder
            FileType = "image/jpeg",
            ProjectId = projectID
        };

        var stream = image.GetFileStream();
        return base.File(stream, image.FileType);
    }

    // Delete
    [HttpDelete]
    public async Task<IActionResult> DeleteImage(int projectID, int planID)
    {

        var image = new ProjectPlanImage
        {
            Id = planID,
            FileName = "planimage.jpg", // Placeholder
            ProjectId = projectID
        };

        var deleted = image.DeleteFromFileSystem();

        if (!deleted)
        {
            return NotFound("File not found.");
        }

        return NoContent();
    }

    private string? GetUserId()
    {
        // Placeholder logic for getting the user ID
        return User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
    }
}