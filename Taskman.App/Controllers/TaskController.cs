namespace Taskman.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taskman;
using Taskman.Models;
using Taskman.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

[ApiController]
[Route("api/project/{projectId}/[controller]")]
[Authorize]  // Require authentication for all actions in this controller
public class TaskController : ControllerBase
{
    private readonly TodoService _taskService;

    public TaskController(TodoService taskService)
    {
        _taskService = taskService;
    }

    // Create a new task
    [HttpPost]
    public async Task<IActionResult> CreateTask(Guid projectId, [FromBody] CreateTodoDto createTodoDto)
    {
        var createdTask = await _taskService.CreateTodoAsync(createTodoDto, projectId, UserId);

        return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
    }

    // Update an existing task
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTodoDto updateTodoDto)
    {
        // Ensure the task exists and the current user has access
        try {
            var task = await _taskService.UpdateTodoAsync(id, updateTodoDto);
            
            return Ok(task);
        } catch (KeyNotFoundException) {
            return NotFound("Task not found.");
        }
    }

    // 3. Get a task by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTaskById(Guid id)
    {
        try {
            var task = await _taskService.GetTodoByIdAsync(id);
            return Ok(task);
        } catch (KeyNotFoundException) {
            return NotFound("Task not found.");
        }
    }

    // 4. Delete a task
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
         try {
            var success = await _taskService.DeleteTodoAsync(id);
            if (!success)
            {
                return NotFound("Task not found or already deleted.");
            }
            return NoContent(); // 204 No Content
        } catch (KeyNotFoundException) {
            return NotFound("Task not found.");
        }
    }

    // 5. List tasks for a project with optional filters
    [HttpGet]
    public async Task<IActionResult> ListTasksForProject(
        Guid projectId, 
        [FromQuery] Guid? assignedUserId = null, 
        [FromQuery] Guid? creatorUserId = null, 
        [FromQuery] Taskman.Models.TodoStatus? status = null)
    {
        // Check that the user has access to the project (this logic can be customized as needed)
        var projects = User.FindAll(Claims.Projects);

        if (projects.Any(p => p.Value == projectId.ToString()))
        {
            var tasks = await _taskService.ListTodosForProjectAsync(projectId, assignedUserId, creatorUserId, status);
            return Ok(tasks);
        }
        return Unauthorized("User does not have access to this project.");
        
    }
}
