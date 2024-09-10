using Taskman.Db;
using Taskman.Models;

namespace Taskman.Services;

public class ProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _authService;

    public ProjectService(ApplicationDbContext context, AuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<Project> GetProjectByIdAsync(Guid id)
    {
        return await _context.Projects.FindAsync(id) ?? throw new KeyNotFoundException("Project not found");
    }

    // 1. Create a new project
    public async Task<Project> CreateProjectAsync(CreateProjectDto data, Guid ownerId)
    {
        var project = new Project
        {
            Name = data.Name,
            Owner = await _authService.GetUserByIdAsync(ownerId)
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    // 2. Add a member to a project
    public async Task<Boolean> AddMemberToProjectAsync(Guid projectId, Guid userId)
    {
        // Ensure the project exists
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found.");
        }

        // Add the user as a member of the project
        if (project.Members.Any(u => u.Id == userId))
        {
            return false;
        }

        // Ensure the user exists
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        project.Members.Add(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
