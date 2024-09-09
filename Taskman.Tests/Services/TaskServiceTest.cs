using Microsoft.EntityFrameworkCore;
using System;
using Xunit;
using Taskman.Infrastructure;
using Taskman.Services;
using Taskman.Tests.TestUtils;
using Taskman.Models;

public class TaskServiceTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private Project _project;

    public TaskServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await SeedDatabase(_fixture);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private User _projectOwner = new User {
        Id = Guid.NewGuid(),
        Username = $"ProjectOwner",
        Email = $"projectowner@test.com",
        Password = $"password123"
    };

    private User _projectMember = new User {
        Id = Guid.NewGuid(),
        Username = $"ProjectOwner",
        Email = $"projectowner@test.com",
        Password = $"password123"
    };

    private async Task SeedDatabase(DatabaseFixture fixture)
    {
        using (var dbContext = fixture.CreateContext())
        {
            dbContext.Users.Add(_projectOwner);
            dbContext.Users.Add(_projectMember);

            var projectService = new ProjectService(dbContext, new AuthService(dbContext));
            _project = await projectService.CreateProjectAsync(new CreateProjectDto(Name: "Test Project"), _projectOwner.Id);
            await projectService.AddMemberToProjectAsync(_project.Id, _projectMember.Id);

            dbContext.SaveChanges();
        }
    }

    [Fact]
    public async Task CreateTask_ShouldAddTaskToDatabase()
    {
        using (var context = _fixture.CreateContext())
        {
            var data = new CreateTodoDto(
                Title: "New Task",
                Description: "New Task Description",
                ProjectId: _project.Id
            );

            var tasksService = new TaskService(context);

            var result = await tasksService.CreateTodoAsync(
                data,
                _project.Id,
                _projectOwner.Id
            );

            var taskInDb = context.Todos.Find(result.Id);
            Assert.NotNull(taskInDb);
            Assert.Equal(data.Title, taskInDb.Title);
            Assert.Equal(data.Description, taskInDb.Description);
            Assert.Equal(data.ProjectId, taskInDb.ProjectId);
            Assert.Equal(_projectOwner.Id, taskInDb.CreatedByUserId);
        }
    }

}
