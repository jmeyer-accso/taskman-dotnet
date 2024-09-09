using Taskman.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Taskman.Models;
using BCrypt.Net;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt; // Add reference to BCrypt.Net library

namespace Taskman.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;

    private readonly byte[] _jwtKey;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
        _jwtKey = Encoding.UTF8.GetBytes(Config.ApplicationSecret);
    }

    public async Task<User> RegisterUserAsync(UserRegistrationDto userDto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
            throw new Exception("Email already in use");

        var user = new User
        {
            Username = userDto.Username,
            Email = userDto.Email,
            Password = userDto.Password
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<string> LoginUserAsync(UserLoginDto loginDto)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == loginDto.Username);
        if (user == null || loginDto.Password != user.Password)
            throw new Exception("Invalid email or password");

        // For simplicity, return a mock token. Implement JWT token generation in production.
        return GenerateJwtToken(user);
    }

    public async Task<User> GetUserByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id) ?? throw new KeyNotFoundException("User not found");
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ]);

        claims.AddClaims(user.AssignedProjects.Select(p => new Claim(Claims.Projects, p.Id.ToString())));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_jwtKey), SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

}
