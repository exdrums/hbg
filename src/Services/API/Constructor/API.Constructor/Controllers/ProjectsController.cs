using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoMapper;
using API.Constructor.Models.DTOs;
using API.Constructor.Models.Entities;
using API.Constructor.Services;

namespace API.Constructor.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly AuthService _authService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(
            IProjectService projectService,
            AuthService authService,
            IMapper mapper,
            ILogger<ProjectsController> logger)
        {
            _projectService = projectService;
            _authService = authService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all projects for the current user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ProjectDto>>> GetProjects()
        {
            try
            {
                var userId = _authService.GetUserId();
                var projects = await _projectService.GetUserProjectsAsync(userId);
                return Ok(_mapper.Map<List<ProjectDto>>(projects));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting projects");
                return StatusCode(500, "An error occurred while retrieving projects");
            }
        }

        /// <summary>
        /// Get a specific project by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(Guid id)
        {
            try
            {
                var userId = _authService.GetUserId();
                var project = await _projectService.GetProjectAsync(id, userId);

                if (project == null)
                {
                    return NotFound();
                }

                return Ok(_mapper.Map<ProjectDto>(project));
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project {ProjectId}", id);
                return StatusCode(500, "An error occurred while retrieving the project");
            }
        }

        /// <summary>
        /// Create a new project
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto createDto)
        {
            try
            {
                var userId = _authService.GetUserId();
                var project = _mapper.Map<ConstructorProject>(createDto);
                project.UserId = userId;

                var createdProject = await _projectService.CreateProjectAsync(project);
                var projectDto = _mapper.Map<ProjectDto>(createdProject);

                return CreatedAtAction(nameof(GetProject), new { id = projectDto.ProjectId }, projectDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, "An error occurred while creating the project");
            }
        }

        /// <summary>
        /// Update an existing project
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProjectDto>> UpdateProject(Guid id, [FromBody] UpdateProjectDto updateDto)
        {
            try
            {
                var userId = _authService.GetUserId();
                var project = _mapper.Map<ConstructorProject>(updateDto);

                var updatedProject = await _projectService.UpdateProjectAsync(id, project, userId);
                return Ok(_mapper.Map<ProjectDto>(updatedProject));
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project {ProjectId}", id);
                return StatusCode(500, "An error occurred while updating the project");
            }
        }

        /// <summary>
        /// Delete a project (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProject(Guid id)
        {
            try
            {
                var userId = _authService.GetUserId();
                await _projectService.DeleteProjectAsync(id, userId);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project {ProjectId}", id);
                return StatusCode(500, "An error occurred while deleting the project");
            }
        }

        /// <summary>
        /// Get all images for a project
        /// </summary>
        [HttpGet("{id}/images")]
        public async Task<ActionResult<List<ImageDto>>> GetProjectImages(Guid id)
        {
            try
            {
                var userId = _authService.GetUserId();
                var images = await _projectService.GetProjectImagesAsync(id, userId);
                return Ok(_mapper.Map<List<ImageDto>>(images));
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting images for project {ProjectId}", id);
                return StatusCode(500, "An error occurred while retrieving images");
            }
        }
    }
}
