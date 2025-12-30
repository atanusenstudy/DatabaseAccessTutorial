using Core.Interfaces;
using Core.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Core.DTO;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;

        public ProjectsController(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        // GET: api/projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            try
            {
                var projects = await _projectRepository.GetAllAsync();
                return Ok(projects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/projects/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            try
            {
                var project = await _projectRepository.GetByIdAsync(id);
                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found");
                }
                return Ok(project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/projects/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Project>>> GetActiveProjects()
        {
            try
            {
                var projects = await _projectRepository.GetActiveProjectsAsync();
                return Ok(projects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/projects/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjectsByStatus(string status)
        {
            try
            {
                var projects = await _projectRepository.GetProjectsByStatusAsync(status);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/projects
        [HttpPost]
        public async Task<ActionResult<Project>> CreateProject(ProjectRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var project = new Project
                {
                    Name = request.Name,
                    Description = request.Description,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = request.Status,
                    Technology = request.Technology,
                    Budget = request.Budget,
                    ClientName = request.ClientName,
                    CreatedAt = DateTime.UtcNow
                };

                var id = await _projectRepository.AddAsync(project);
                project.Id = id;

                return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/projects/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, ProjectRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingProject = await _projectRepository.GetByIdAsync(id);
                if (existingProject == null)
                {
                    return NotFound($"Project with ID {id} not found");
                }

                existingProject.Name = request.Name;
                existingProject.Description = request.Description;
                existingProject.StartDate = request.StartDate;
                existingProject.EndDate = request.EndDate;
                existingProject.Status = request.Status;
                existingProject.Technology = request.Technology;
                existingProject.Budget = request.Budget;
                existingProject.ClientName = request.ClientName;
                existingProject.UpdatedAt = DateTime.UtcNow;

                var updated = await _projectRepository.UpdateAsync(existingProject);
                if (!updated)
                {
                    return StatusCode(500, "Failed to update project");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/projects/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var project = await _projectRepository.GetByIdAsync(id);
                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found");
                }

                var deleted = await _projectRepository.DeleteAsync(id);
                if (!deleted)
                {
                    return StatusCode(500, "Failed to delete project");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}