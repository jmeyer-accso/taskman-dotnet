using Taskman.Db;
using Microsoft.EntityFrameworkCore;
using Taskman.Models;

namespace Taskman.Services;

public class TodoService
{
    private readonly ApplicationDbContext _context;

    public TodoService(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Create a new task
    public async Task<Todo> CreateTodoAsync(CreateTodoDto createTodo, Guid projectId, Guid createdByUserId)
    {
        var todo = new Todo
        {
            Title = createTodo.Title,
            Description = createTodo.Description,
            ProjectId = projectId,
            CreatedByUserId = createdByUserId,
        };

        _context.Todos.Add(todo);
        await _context.SaveChangesAsync();
        return todo;
    }

    // 2. Update a task
    public async Task<Todo> UpdateTodoAsync(Guid todoId, UpdateTodoDto updateTodo)
    {
        var todo = await _context.Todos.FindAsync(todoId);
        if (todo == null)
        {
            throw new KeyNotFoundException("Todo not found");
        }

        todo.Title = updateTodo.Title ?? todo.Title;
        todo.Description = updateTodo.Description ?? todo.Description;
        todo.AssignedUserId = updateTodo.AssignedUserId ?? todo.AssignedUserId;
        todo.Status = updateTodo.Status ?? todo.Status;
        todo.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return todo;
    }

    // 3. Get a todo by ID
    public async Task<Todo> GetTodoByIdAsync(Guid todoId)
    {
        var todo = await _context.Todos.FirstAsync();
        if (todo == null)
        {
            throw new KeyNotFoundException("Todo not found");
        }
        return todo;
    }

    // 4. Delete a todo
    public async Task<bool> DeleteTodoAsync(Guid todoId)
    {
        var todo = await _context.Todos.FindAsync(todoId);
        if (todo == null)
        {
            return false;
        }

        _context.Todos.Remove(todo);
        await _context.SaveChangesAsync();
        return true;
    }

    // 5. List todos for a project with optional filters
    public async Task<List<Todo>> ListTodosForProjectAsync(
        Guid projectId,
        Guid? assignedUserId = null,
        Guid? creatorUserId = null,
        TodoStatus? status = null,
        int? limit = null,
        int? offset = null,
        string? orderBy = null,
        bool? descending = false)
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

        if (limit.HasValue)
        {
            query = query.Take(limit.Value);
        }

        if (offset.HasValue)
        {
            query = query.Skip(offset.Value);
        }

        var todos = await query.ToListAsync();

        if (orderBy != null)
        {
            Func<Todo, Object> keySelector = todo => orderBy switch
            {
                "title" => todo.Title,
                "description" => todo.Description,
                "status" => todo.Status,
                "createdat" => todo.CreatedAt,
                "updatedat" => todo.UpdatedAt,
                _ => throw new ArgumentException($"Invalid orderBy value: {orderBy}", nameof(orderBy))
            };
            
            todos = descending ?? false
                ? todos.OrderByDescending(keySelector).ToList()
                : todos.OrderBy(keySelector).ToList();
        }

        return todos;
    }
}
