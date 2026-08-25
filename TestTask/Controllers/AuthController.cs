using Application.Authentication.DTOs;
using Application.Authentication.Interfaces;
using Infrastructure.Data;
using Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _manager;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly IdentityDbContext _identityContext;

        public AuthController(UserManager<ApplicationUser> manager, ApplicationDbContext context,
            SignInManager<ApplicationUser> signInManager, IJwtService jwtService, IdentityDbContext identityContext)
        {
            _manager = manager;
            _context = context;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _identityContext = identityContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request, CancellationToken cancellationToken)
        {
            var existingUser = await _manager.FindByEmailAsync(request.Email);

            if (existingUser is not null)
            {
                return BadRequest("User with this email already exists");
            }

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email
            };

            var createUserResult = await _manager.CreateAsync(
                user,
                request.Password);

            if (!createUserResult.Succeeded)
            {
                return BadRequest(
                    createUserResult.Errors.Select(x => x.Description));
            }

            var roleResult = await _manager.AddToRoleAsync(
                user,
                "User");

            if (!roleResult.Succeeded)
            {
                await _manager.DeleteAsync(user);

                return BadRequest(
                    roleResult.Errors.Select(x => x.Description));
            }

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginDto dto)
        {
            var user = await _manager.FindByEmailAsync(
                dto.Email);

            if (user is null)
            {
                return Unauthorized();
            }

            var result =
                await _signInManager.CheckPasswordSignInAsync(
                    user,
                    dto.Password,
                    lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            var roles =
                await _manager.GetRolesAsync(user);

            var details = new UserTokenDetails
            {
                UserId = user.Id,
                Email = user.Email!,
                UserName = user.UserName,
                Roles = roles
            };

            var accessToken =
                _jwtService.GenerateAccessToken(details);

            var refreshToken =
                _jwtService.GenerateRefreshToken();

            var entity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _identityContext.RefreshTokens.Add(entity);

            await _identityContext.SaveChangesAsync();

            var claims = new List<Claim>
    {
        new Claim(
            ClaimTypes.NameIdentifier,
            user.Id.ToString()),

        new Claim(
            ClaimTypes.Name,
            user.UserName ?? user.Email!),

        new Claim(
            ClaimTypes.Email,
            user.Email!)
    };

            foreach (var role in roles)
            {
                claims.Add(
                    new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(
                claims,
                "MvcCookie");

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                "MvcCookie",
                principal);


            var response = new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return Ok(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutRequest request)
        {
            var refreshToken =
                await _identityContext.RefreshTokens
                    .FirstOrDefaultAsync(
                        x => x.Token == request.RefreshToken);

            if (refreshToken is null)
            {
                return Unauthorized();
            }

            refreshToken.IsRevoked = true;

            await _identityContext.SaveChangesAsync();
            await HttpContext.SignOutAsync("MvcCookie");
            return NoContent();
        }
    }
}
