using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Taskman.Services;
using Taskman.Models;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all actions
public class ProjectController : ControllerBase
{
    private readonly ProjectService _projectService;

    public ProjectController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    private Guid GetAuthUserId()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        return Guid.Parse(userIdClaim.Value);
    }

    // 1. Create a new project
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto data)
    {
        var createdProject = await _projectService.CreateProjectAsync(data, GetAuthUserId());

        return CreatedAtAction(nameof(GetProjectById), new { id = createdProject.Id }, createdProject);
    }

    // Get a project by ID (useful for verifying the project was created)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProjectById(Guid id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
        {
            return NotFound("Project not found.");
        }
        return Ok(project);
    }

    // 2. Add a member to a project
    [HttpPost("{projectId}/members")]
    public async Task<IActionResult> AddMemberToProject(Guid projectId, [FromBody] AddMemberDto addMemberDto)
    {
        Guid ownerId = GetAuthUserId();

        // Verify the current user is the owner of the project
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null)
        {
            return NotFound("Project not found.");
        }

        if (project.Owner.Id != ownerId)
        {
            return Forbid("You do not have permission to add members to this project.");
        }

        // Add the member to the project
        var projectMember = await _projectService.AddMemberToProjectAsync(projectId, addMemberDto.UserId);
        return Ok(projectMember);
    }
}

public class AddMemberDto
{
    public Guid UserId { get; set; }
}
