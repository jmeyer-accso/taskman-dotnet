using FluentAssertions;
using Taskman.Db;
using Taskman.Services;
using Taskman.Tests.TestUtils;
using Taskman.Models;
using Taskman.Controllers;
using Microsoft.AspNetCore.Authorization;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

public class ProjectControllerTest : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly Mock<IAuthorizationService> _mockAuthService = new();

    public ProjectControllerTest(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _mockAuthService.Setup(auth => 
            auth.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()).Result)
            .Returns(AuthorizationResult.Success());
    }

    private ProjectController createController(ApplicationDbContext context) {
        var controller = new ProjectController(
            new ProjectService(context, new AuthService(context)), _mockAuthService.Object);
        
        return controller;
    }

    [Fact]
    public async Task GetProjectById_ShouldReturnTheProject()
    {
        using var context = _fixture.CreateContext();
        var controller = createController(context);

        var result = await controller.GetProjectById(DatabaseFixture.Project.Id);
        result.Should().BeOfType<OkObjectResult>();
    }

}
