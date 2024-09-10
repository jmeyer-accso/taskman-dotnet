namespace Taskman.Tests.TestUtils;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Taskman.Db;
using Taskman.Models;
using Taskman.Services;

public class DatabaseFixture
{
    private static readonly object _lock = new();
    private static bool _databaseInitialized;

    public static User ProjectOwner = new()
    {
        Id = Guid.NewGuid(),
        UserName = $"ProjectOwner",
        Email = $"projectowner@test.com",
        Password = $"password123"
    };

    public static  User ProjectMember = new()
    {
        Id = Guid.NewGuid(),
        UserName = $"ProjectMember",
        Email = $"projectmember@test.com",
        Password = $"password123"
    };

    public static User NonProjectMember = new()
    {
        Id = Guid.NewGuid(),
        UserName = $"NonProjectMember",
        Email = $"nonprojectmember@test.com",
        Password = $"password123"
    };

    public static Project Project = new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Project",
        Owner = ProjectOwner,
        Members = [ProjectMember]
    };


    public DatabaseFixture()
    {
        lock (_lock)
        {
            if (!_databaseInitialized)
            {
                using (var context = CreateContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();
                    SeedProjectAndUsers(context);
                }

                _databaseInitialized = true;
            }
        }
    }

    public ApplicationDbContext CreateContext()
    {
        var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }

    private void SeedProjectAndUsers(ApplicationDbContext dbContext)
    {
        dbContext.Users.Add(ProjectOwner);
        dbContext.Users.Add(ProjectMember);
        dbContext.Users.Add(NonProjectMember);
        dbContext.Projects.Add(Project);
        SeedTasks(dbContext);

        dbContext.SaveChanges();
    }


    private void SeedTasks(ApplicationDbContext context)
    {
        TodoTitles.ForEach(title =>
        {
            var todo = new Todo
            {
                Title = title,
                Description = $"Thou shalt {title}",
                ProjectId = Project.Id,
                CreatedByUserId = ProjectOwner.Id
            };

            context.Todos.Add(todo);
        });
    }


    public static readonly List<string> TodoTitles =
    [
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

