using Application.Authentication.DTOs;
using Application.Authentication.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Identity
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(UserTokenDetails details)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, details.UserId.ToString()),

            new Claim(JwtRegisteredClaimNames.Sub, details.UserId.ToString()),

            new Claim(JwtRegisteredClaimNames.Email, details.Email!),

            new Claim(ClaimTypes.Name, details.UserName ?? details.Email!),
        };
            foreach (var role in details.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);
                
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(
                        _configuration["Jwt:AccessTokenMinutes"]!, CultureInfo.InvariantCulture)),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));
        }
    }
}
