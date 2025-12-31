using IdentityService.Data;
using IdentityService.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.DTOs;
using BCrypt.Net;

namespace IdentityService.Services;

public class AuthService : IAuthService
{
    private readonly IdentityDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(IdentityDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        // Assign default role
        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole != null)
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = userRole.Id
            });
        }

        await _context.SaveChangesAsync();

        // Generate token
        var roles = await GetUserRolesAsync(user.Id);
        var token = _tokenService.GenerateToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var roles = await GetUserRolesAsync(user.Id);
        var token = _tokenService.GenerateToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles
            }
        };
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        return _tokenService.ValidateToken(token);
    }

    private async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role.Name)
            .ToListAsync();
    }
}

