using Taskman.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Taskman.Models;

namespace Taskman.Services;

public class TaskService
{
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Create a new task
    public async Task<Todo> CreateTodoAsync(CreateTodoDto createTask, Guid projectId, Guid createdByUserId)
    {
        var task = new Todo
        {
            Title = createTask.Title,
            Description = createTask.Description,
            ProjectId = projectId,
            CreatedByUserId = createdByUserId,
        };

        _context.Todos.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    // 2. Update a task
    public async Task<Todo> UpdateTaskAsync(Guid taskId, UpdateTodoDto updateTask)
    {
        var task = await _context.Todos.FindAsync(taskId);
        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        task.Title = updateTask.Title ?? task.Title;
        task.Description = updateTask.Description ?? task.Description;
        task.AssignedUserId = updateTask.AssignedUserId ?? task.AssignedUserId;
        task.Status = updateTask.Status ?? task.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return task;
    }

    // 3. Get a task by ID
    public async Task<Todo> GetTaskByIdAsync(Guid taskId)
    {
        var task = await _context.Todos.FindAsync(taskId);
        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }
        return task;
    }

    // 4. Delete a task
    public async Task<bool> DeleteTaskAsync(Guid taskId)
    {
        var task = await _context.Todos.FindAsync(taskId);
        if (task == null)
        {
            return false;
        }

        _context.Todos.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    // 5. List tasks for a project with optional filters
    public async Task<List<Todo>> ListTasksForProjectAsync(
        Guid projectId,
        Guid? assignedUserId = null,
        Guid? creatorUserId = null,
        TodoStatus? status = null)
    {
        var query = _context.Todos.Where(t => t.ProjectId == projectId);

        if (assignedUserId is Guid assignedUserIdValue)
        {
            query = query.Where(t => t.AssignedUserId == assignedUserIdValue);
        }

        if (creatorUserId is Guid creatorUserIdValue)
        {
            query = query.Where(t => t.CreatedByUserId == creatorUserIdValue);
        }

        if (status is TodoStatus statusValue)
        {
            query = query.Where(t => t.Status == statusValue);
        }

        return await query.ToListAsync();
    }
}
