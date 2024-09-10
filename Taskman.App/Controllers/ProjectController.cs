namespace Taskman.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Taskman.Services;
using Taskman.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all actions
public class ProjectController : ControllerBase
{
    private readonly ProjectService _projectService;
    private readonly IAuthorizationService _authService;

    public ProjectController(ProjectService projectService, IAuthorizationService authService)
    {
        _projectService = projectService;
        _authService = authService;
    }

    // 1. Create a new project
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto data)
    {
        var createdProject = await _projectService.CreateProjectAsync(data, UserId);

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
        var authResult = await _authService.AuthorizeAsync(User, project, "ProjectMember");

        if (!authResult.Succeeded)
        {
            return Forbid("You do not have permission to view this project.");
        }

        return Ok(project);
    }

    // 2. Add a member to a project
    [HttpPost("{projectId}/members")]
    public async Task<IActionResult> AddMemberToProject(Guid projectId, [FromBody] AddMemberDto addMemberDto)
    {
        // Verify the current user is the owner of the project
        var project = await _projectService.GetProjectByIdAsync(projectId);
        if (project == null)
        {
            return NotFound("Project not found.");
        }

        if (project.Owner.Id != UserId)
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
