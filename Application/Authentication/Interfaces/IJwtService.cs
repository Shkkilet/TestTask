
using Application.Authentication.DTOs;

namespace Application.Authentication.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(UserTokenDetails details);
        string GenerateRefreshToken();
    }
}
