using Api.Controllers;
using Application.Authentication.DTOs;
using Application.Authentication.Interfaces;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace TestTask.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManager;
    private readonly Mock<IJwtService> _jwtService;
    private readonly ApplicationDbContext _context;
    private readonly IdentityDbContext _identityContext;

    public AuthControllerTests()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();

        _userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        var contextAccessor =
            new Mock<IHttpContextAccessor>();

        var userPrincipalFactory =
            new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

        _signInManager = new Mock<SignInManager<ApplicationUser>>(
            _userManager.Object,
            contextAccessor.Object,
            userPrincipalFactory.Object,
            null!,
            null!,
            null!,
            null!);

        _jwtService = new Mock<IJwtService>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var identityOptions = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _identityContext = new IdentityDbContext(identityOptions);
    }

    private AuthController CreateController()
    {
        var controller = new AuthController(
            _userManager.Object,
            _context,
            _signInManager.Object,
            _jwtService.Object,
            _identityContext);

        var httpContext = new DefaultHttpContext();

        var authenticationService =
            new Mock<IAuthenticationService>();

        authenticationService
            .Setup(x => x.SignInAsync(
                It.IsAny<HttpContext>(),
                "MvcCookie",
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        authenticationService
            .Setup(x => x.SignOutAsync(
                It.IsAny<HttpContext>(),
                "MvcCookie",
                It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask);

        httpContext.RequestServices =
            new ServiceCollection()
                .AddSingleton(authenticationService.Object)
                .BuildServiceProvider();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    [Fact]
    public async Task Register_ReturnsNoContent_WhenRegistrationSucceeds()
    {
        var controller = CreateController();

        var request = new RegisterDto
        {
            UserName = "alice",
            Email = "alice@example.com",
            Password = "Password!1"
        };

        _userManager
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManager
            .Setup(x => x.CreateAsync(
                It.IsAny<ApplicationUser>(),
                request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                "User"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await controller.Register(
            request,
            CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();

        _userManager.Verify(
            x => x.CreateAsync(
                It.Is<ApplicationUser>(u =>
                    u.UserName == "alice" &&
                    u.Email == "alice@example.com"),
                "Password!1"),
            Times.Once);

        _userManager.Verify(
            x => x.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                "User"),
            Times.Once);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenEmailAlreadyExists()
    {
        var controller = CreateController();

        var existingUser = new ApplicationUser
        {
            Email = "alice@example.com",
            UserName = "alice"
        };

        var request = new RegisterDto
        {
            UserName = "alice",
            Email = "alice@example.com",
            Password = "Password!1"
        };

        _userManager
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(existingUser);

        var result = await controller.Register(
            request,
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();

        _userManager.Verify(
            x => x.CreateAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenCreateUserFails()
    {
        var controller = CreateController();

        var request = new RegisterDto
        {
            UserName = "alice",
            Email = "alice@example.com",
            Password = "Password!1"
        };

        var errors = new[]
        {
            new IdentityError
            {
                Code = "PasswordTooWeak",
                Description = "Password is too weak."
            }
        };

        _userManager
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManager
            .Setup(x => x.CreateAsync(
                It.IsAny<ApplicationUser>(),
                request.Password))
            .ReturnsAsync(IdentityResult.Failed(errors));

        var result = await controller.Register(
            request,
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = (BadRequestObjectResult)result;

        badRequest.Value.Should()
            .BeEquivalentTo(new[]
            {
                "Password is too weak."
            });

        _userManager.Verify(
            x => x.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                "User"),
            Times.Never);
    }

    [Fact]
    public async Task Register_DeletesUser_WhenAddingRoleFails()
    {
        var controller = CreateController();

        var request = new RegisterDto
        {
            UserName = "alice",
            Email = "alice@example.com",
            Password = "Password!1"
        };

        _userManager
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _userManager
            .Setup(x => x.CreateAsync(
                It.IsAny<ApplicationUser>(),
                request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.AddToRoleAsync(
                It.IsAny<ApplicationUser>(),
                "User"))
            .ReturnsAsync(
                IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Role does not exist."
                    }));

        _userManager
            .Setup(x => x.DeleteAsync(
                It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await controller.Register(
            request,
            CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();

        _userManager.Verify(
            x => x.DeleteAsync(
                It.IsAny<ApplicationUser>()),
            Times.Once);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserDoesNotExist()
    {
        var controller = CreateController();

        var dto = new LoginDto
        {
            Email = "unknown@example.com",
            Password = "Password!1"
        };

        _userManager
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((ApplicationUser?)null);

        var result = await controller.Login(dto);

        result.Result.Should()
            .BeOfType<UnauthorizedResult>();

        _signInManager.Verify(
            x => x.CheckPasswordSignInAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>(),
                It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsInvalid()
    {
        var controller = CreateController();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "alice@example.com",
            UserName = "alice"
        };

        var dto = new LoginDto
        {
            Email = "alice@example.com",
            Password = "WrongPassword!1"
        };

        _userManager
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _signInManager
            .Setup(x => x.CheckPasswordSignInAsync(
                user,
                dto.Password,
                false))
            .ReturnsAsync(
                Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var result = await controller.Login(dto);

        result.Result.Should()
            .BeOfType<UnauthorizedResult>();

        _jwtService.Verify(
            x => x.GenerateAccessToken(
                It.IsAny<UserTokenDetails>()),
            Times.Never);
    }

    [Fact]
    public async Task Login_ReturnsTokens_WhenCredentialsAreValid()
    {
        var controller = CreateController();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "alice@example.com",
            UserName = "alice"
        };

        var dto = new LoginDto
        {
            Email = "alice@example.com",
            Password = "Password!1"
        };

        var roles = new List<string>
        {
            "User"
        };

        _userManager
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _signInManager
            .Setup(x => x.CheckPasswordSignInAsync(
                user,
                dto.Password,
                false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        _userManager
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(roles);

        _jwtService
            .Setup(x => x.GenerateAccessToken(
                It.IsAny<UserTokenDetails>()))
            .Returns("access-token");

        _jwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        var result = await controller.Login(dto);

        result.Result.Should()
            .BeOfType<OkObjectResult>();

        var okResult = (OkObjectResult)result.Result!;

        var response = okResult.Value
            .Should()
            .BeOfType<AuthResponse>()
            .Subject;

        response.AccessToken.Should()
            .Be("access-token");

        response.RefreshToken.Should()
            .Be("refresh-token");

        var savedToken =
            await _identityContext.RefreshTokens.SingleAsync();

        savedToken.UserId.Should().Be(user.Id);
        savedToken.Token.Should().Be("refresh-token");
        savedToken.IsRevoked.Should().BeFalse();

        _jwtService.Verify(
            x => x.GenerateAccessToken(
                It.Is<UserTokenDetails>(d =>
                    d.UserId == user.Id &&
                    d.Email == user.Email &&
                    d.UserName == user.UserName &&
                    d.Roles.SequenceEqual(roles))),
            Times.Once);
    }

    [Fact]
    public async Task Logout_ReturnsUnauthorized_WhenRefreshTokenDoesNotExist()
    {
        var controller = CreateController();

        var request = new LogoutRequest
        {
            RefreshToken = "unknown-token"
        };

        var result = await controller.Logout(request);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken_WhenTokenExists()
    {
        var controller = CreateController();

        var userId = Guid.NewGuid();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _identityContext.RefreshTokens.Add(refreshToken);
        await _identityContext.SaveChangesAsync();

        var request = new LogoutRequest
        {
            RefreshToken = "refresh-token"
        };

        var result = await controller.Logout(request);

        result.Should().BeOfType<NoContentResult>();

        var savedToken =
            await _identityContext.RefreshTokens
                .SingleAsync();

        savedToken.IsRevoked.Should().BeTrue();
    }
}