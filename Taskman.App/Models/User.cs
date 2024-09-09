using Microsoft.AspNetCore.Identity;

namespace Taskman.Models;

public class User : IdentityUser<Guid>
{
    public required string Username { get; set; }
    public required string Password { get; set; }

    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public ICollection<Project> AssignedProjects { get; set; } = new List<Project>();
}

public static class Claims
{
    public const string Projects = "projects";
}
