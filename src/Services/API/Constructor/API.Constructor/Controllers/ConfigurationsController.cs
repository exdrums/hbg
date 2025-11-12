using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoMapper;
using API.Constructor.Models.DTOs;
using API.Constructor.Models.Entities;
using API.Constructor.Models.Enums;
using API.Constructor.Services;

namespace API.Constructor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConfigurationsController : ControllerBase
    {
        private readonly IConfigurationService _configService;
        private readonly IProjectService _projectService;
        private readonly AuthService _authService;
        private readonly IMapper _mapper;
        private readonly ILogger<ConfigurationsController> _logger;

        public ConfigurationsController(
            IConfigurationService configService,
            IProjectService projectService,
            AuthService authService,
            IMapper mapper,
            ILogger<ConfigurationsController> logger)
        {
            _configService = configService;
            _projectService = projectService;
            _authService = authService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get a specific configuration by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ConfigurationDto>> GetConfiguration(Guid id)
        {
            try
            {
                var configuration = await _configService.GetConfigurationAsync(id);

                if (configuration == null)
                {
                    return NotFound();
                }

                // Verify user owns the project
                var userId = _authService.GetUserId();
                if (configuration.Project.UserId != userId)
                {
                    return NotFound();
                }

                return Ok(_mapper.Map<ConfigurationDto>(configuration));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configuration {ConfigurationId}", id);
                return StatusCode(500, "An error occurred while retrieving the configuration");
            }
        }

        /// <summary>
        /// Save a new configuration for a project
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ConfigurationDto>> SaveConfiguration([FromBody] CreateConfigurationDto createDto)
        {
            try
            {
                var userId = _authService.GetUserId();
                var project = await _projectService.GetProjectAsync(createDto.ProjectId, userId);

                if (project == null)
                {
                    return BadRequest("Project not found or access denied");
                }

                var configuration = await _configService.SaveConfigurationAsync(
                    createDto.ProjectId,
                    createDto.ConfigurationName,
                    createDto.FormData,
                    project.JewelryType
                );

                var configDto = _mapper.Map<ConfigurationDto>(configuration);

                return CreatedAtAction(nameof(GetConfiguration), new { id = configDto.ConfigurationId }, configDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration");
                return StatusCode(500, "An error occurred while saving the configuration");
            }
        }

        /// <summary>
        /// Generate an image from a configuration
        /// </summary>
        [HttpPost("{id}/generate")]
        public async Task<ActionResult<ImageDto>> GenerateImage(Guid id, [FromBody] GenerateImageDto generateDto)
        {
            try
            {
                var configuration = await _configService.GetConfigurationAsync(id);

                if (configuration == null)
                {
                    return NotFound("Configuration not found");
                }

                // Verify user owns the project
                var userId = _authService.GetUserId();
                if (configuration.Project.UserId != userId)
                {
                    return NotFound();
                }

                var image = await _configService.GenerateAndSaveImageAsync(
                    id,
                    generateDto.AspectRatio ?? "1:1",
                    GenerationSource.Form
                );

                return Ok(_mapper.Map<ImageDto>(image));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating image for configuration {ConfigurationId}", id);
                return StatusCode(500, $"An error occurred while generating the image: {ex.Message}");
            }
        }
    }
}
