using Microsoft.EntityFrameworkCore;
using System;
using Xunit;
using FluentAssertions;
using Taskman.Db;
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

    private User _projectOwner = new User
    {
        Id = Guid.NewGuid(),
        UserName = $"ProjectOwner",
        Email = $"projectowner@test.com",
        Password = $"password123"
    };

    private User _projectMember = new User
    {
        Id = Guid.NewGuid(),
        UserName = $"ProjectOwner",
        Email = $"projectowner@test.com",
        Password = $"password123"
    };


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

            var tasksService = new TodoService(context);

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

    [Fact]
    public async Task ListTasks_ShouldReturnAllTasksForProject()
    {
        using (var context = _fixture.CreateContext())
        {
            var tasksService = new TodoService(context);

            var result = await tasksService.ListTodosForProjectAsync(_project.Id);

            result.Select(task => task.Title).Should().BeEquivalentTo(_taskTitles);

        }
    }

    [Fact]
    public async Task ListTasks_ShouldReturnTasksInSpecifiedOrder()
    {
        using var context = _fixture.CreateContext();
        var tasksService = new TodoService(context);

        var result = await tasksService.ListTodosForProjectAsync(_project.Id, orderBy: "title");

        result.Select(task => task.Title).Should().BeEquivalentTo(_taskTitles.Order());
    }

    [Fact]
    public async Task ListTasks_ShouldReturnLimitedResults()
    {
        using var context = _fixture.CreateContext();
        var tasksService = new TodoService(context);

        var result = await tasksService.ListTodosForProjectAsync(_project.Id, orderBy: "title", limit: 5);

        result.Select(task => task.Title).Should().BeEquivalentTo(_taskTitles.Order().Take(5));
    }

    [Fact]
    public async Task ListTasks_ShouldReturnOffsetResults()
    {
        using var context = _fixture.CreateContext();
        var tasksService = new TodoService(context);

        var result = await tasksService.ListTodosForProjectAsync(_project.Id, orderBy: "title", limit: 5, offset: 5);

        result.Select(task => task.Title).Should().BeEquivalentTo(_taskTitles.Order().Skip(5).Take(5));
    }

    private async Task SeedDatabase(DatabaseFixture fixture)
    {
        using var dbContext = fixture.CreateContext();
        dbContext.Users.Add(_projectOwner);
        dbContext.Users.Add(_projectMember);

        var projectService = new ProjectService(dbContext, new AuthService(dbContext));
        _project = await projectService.CreateProjectAsync(new CreateProjectDto(Name: "Test Project"), _projectOwner.Id);
        await projectService.AddMemberToProjectAsync(_project.Id, _projectMember.Id);

        dbContext.SaveChanges();

        SeedTasks(dbContext);
    }

    private void SeedTasks(ApplicationDbContext context)
    {
        _taskTitles.ForEach(title =>
        {
            var todo = new Todo
            {
                Title = title,
                Description = $"Thou shalt {title}",
                ProjectId = _project.Id,
                CreatedByUserId = _projectOwner.Id
            };

            context.Todos.Add(todo);

        });

        context.SaveChanges();
    }

    readonly List<String> _taskTitles = [
        "Mow the lawn",
        "Build a bridge",
        "Brush teeth",
        "Do laundry",
        "Clean the house",
        "Wash the car",
        "Walk the dog",
        "Take out the trash",
        "Make dinner",
        "Do the dishes",
        "Conquer the world",
        "Save the princess",
        "Defeat the dragon",
        "Find the treasure",
        "Rescue the hostages",
        "Save the day",
    ];

}
