using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Taskman.Controllers;
using Taskman.Db;
using Taskman.Models;

namespace Taskman.Auth;

public class ProjectAuthorizationHandler : 
    AuthorizationHandler<ProjectMemberRequirement, Project>
{
    private ApplicationDbContext _dbContext;

    public ProjectAuthorizationHandler(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectMemberRequirement requirement,
        Project resource)
    {
        var userId = context.User.GetUserId();
        if (userId == null) return;

        var user = await _dbContext.Users
            .Include(u => u.AssignedProjects)
            .FirstAsync(u => u.Id == userId);

        if (user == null) return;

        if (user.AssignedProjects.Any(p => p.Id == resource.Id)) {
            context.Succeed(requirement);
        }
    }
}

public class ProjectMemberRequirement : IAuthorizationRequirement { }