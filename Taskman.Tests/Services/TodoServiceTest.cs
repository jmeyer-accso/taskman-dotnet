using FluentAssertions;
using Taskman.Db;
using Taskman.Services;
using Taskman.Tests.TestUtils;
using Taskman.Models;
using Microsoft.EntityFrameworkCore;

public class TodoServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public TodoServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateTodo_ShouldAddTodoToDatabase()
    {
        await withRollback(async context =>
        {
            var data = new CreateTodoDto(
                Title: "New Task",
                Description: "New Task Description",
                ProjectId: DatabaseFixture.Project.Id
            );

            var todoService = new TodoService(context);

            var result = await todoService.CreateTodoAsync(
                data,
                DatabaseFixture.Project.Id,
                DatabaseFixture.ProjectOwner.Id
            );

            var todoInDb = context.Todos.Find(result.Id);
            Assert.NotNull(todoInDb);
            Assert.Equal(data.Title, todoInDb.Title);
            Assert.Equal(data.Description, todoInDb.Description);
            Assert.Equal(data.ProjectId, todoInDb.ProjectId);
            Assert.Equal(DatabaseFixture.ProjectOwner.Id, todoInDb.CreatedByUserId);
        });
    }

    private async Task withRollback(Func<ApplicationDbContext, Task> action)
    {
        using var context = _fixture.CreateContext();
        var transaction = await context.Database.BeginTransactionAsync();
        await action(context);
        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task ListTodos_ShouldReturnAllTodosForProject()
    {
        using (var context = _fixture.CreateContext())
        {
            var todoService = new TodoService(context);

            var result = await todoService.ListTodosForProjectAsync(DatabaseFixture.Project.Id);

            result.Select(todo => todo.Title).Should().BeEquivalentTo(DatabaseFixture.TodoTitles);
        }
    }

    [Fact]
    public async Task ListTodos_ShouldReturnTodosInSpecifiedOrder()
    {
        using var context = _fixture.CreateContext();
        var todoService = new TodoService(context);

        var result = await todoService.ListTodosForProjectAsync(DatabaseFixture.Project.Id);

        result.Select(todo => todo.Title).Should().BeEquivalentTo(DatabaseFixture.TodoTitles.Order());
    }

    [Fact]
    public async Task ListTodos_ShouldReturnLimitedResults()
    {
        using var context = _fixture.CreateContext();
        var todoService = new TodoService(context);

        var result = await todoService.ListTodosForProjectAsync(DatabaseFixture.Project.Id, limit: 5);
        var actual = result.Select(todo => todo.Title).ToList();
        var expected = DatabaseFixture.TodoTitles.Order().Take(5).ToList();
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task ListTodos_ShouldReturnOffsetResults()
    {
        using var context = _fixture.CreateContext();
        var todoService = new TodoService(context);

        var result = await todoService.ListTodosForProjectAsync(DatabaseFixture.Project.Id, limit: 5, offset: 5);

        result.Select(todo => todo.Title).Should().BeEquivalentTo(DatabaseFixture.TodoTitles.Order().Skip(5).Take(5));
    }

}
