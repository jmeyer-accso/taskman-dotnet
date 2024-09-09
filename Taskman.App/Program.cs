using System.Text;
using Taskman.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Taskman.Services;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddDbContext<ApplicationDbContext>((provider, options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var logger = provider.GetRequiredService<ILogger<ApplicationDbContext>>();
    logger.LogInformation($"Connection string: {connectionString}");
    options.UseNpgsql(connectionString);
});


builder.Services.AddScoped<AuthService>();
builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Config.ApplicationSecret))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
