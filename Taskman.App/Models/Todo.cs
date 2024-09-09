namespace Taskman.Models;


public record CreateTodoDto(string Title, string Description, Guid ProjectId, Guid? AssignedUserId = null);

public record UpdateTodoDto(string? Title = null, string? Description = null, Guid? AssignedUserId = null, TodoStatus? Status = null);

public class Todo
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = "";

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public TodoStatus Status { get; set; } = TodoStatus.Open;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum TodoStatus
{
    Open,
    InProgress,
    Closed
}