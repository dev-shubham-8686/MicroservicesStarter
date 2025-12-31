using IdentityService.Models;

namespace IdentityService.Services;

public interface ITokenService
{
    string GenerateToken(User user, List<string> roles);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
}

