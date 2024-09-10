namespace Taskman.Controllers;

public class ControllerBase : Microsoft.AspNetCore.Mvc.ControllerBase
{
    protected Guid UserId => User.GetUserId() ?? throw new UnauthorizedAccessException("User is not authenticated.");
}