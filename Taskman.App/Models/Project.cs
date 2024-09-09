namespace Taskman.Models;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public User Owner { get; set; } = null!;

    public List<User> Members { get; set; } = new List<User>();
}

public record CreateProjectDto(string Name);
