using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using API.Constructor.Data;
using API.Constructor.Models.DTOs;
using API.Constructor.Services;

namespace API.Constructor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly ConstructorDbContext _context;
        private readonly AuthService _authService;
        private readonly IMapper _mapper;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(
            ConstructorDbContext context,
            AuthService authService,
            IMapper mapper,
            ILogger<ImagesController> logger)
        {
            _context = context;
            _authService = authService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get a specific image by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ImageDto>> GetImage(Guid id)
        {
            try
            {
                var userId = _authService.GetUserId();

                var image = await _context.GeneratedImages
                    .Include(i => i.Configuration)
                    .ThenInclude(c => c.Project)
                    .FirstOrDefaultAsync(i => i.ImageId == id && !i.IsDeleted);

                if (image == null || image.Configuration.Project.UserId != userId)
                {
                    return NotFound();
                }

                return Ok(_mapper.Map<ImageDto>(image));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image {ImageId}", id);
                return StatusCode(500, "An error occurred while retrieving the image");
            }
        }

        /// <summary>
        /// Delete an image (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteImage(Guid id)
        {
            try
            {
                var userId = _authService.GetUserId();

                var image = await _context.GeneratedImages
                    .Include(i => i.Configuration)
                    .ThenInclude(c => c.Project)
                    .FirstOrDefaultAsync(i => i.ImageId == id);

                if (image == null || image.Configuration.Project.UserId != userId)
                {
                    return NotFound();
                }

                image.IsDeleted = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Image {ImageId} marked as deleted", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image {ImageId}", id);
                return StatusCode(500, "An error occurred while deleting the image");
            }
        }
    }
}
