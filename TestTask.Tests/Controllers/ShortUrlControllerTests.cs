using Api.Controllers;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace TestTask.Tests.Controllers
{
    public class ShortUrlControllerTests
    {
        private static ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private static ClaimsPrincipal CreateUser(
            Guid userId,
            bool isAdmin = false)
        {
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userId.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    "test@example.com")
            };

            if (isAdmin)
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        "Admin"));
            }

            var identity = new ClaimsIdentity(
                claims,
                "TestAuthentication");

            return new ClaimsPrincipal(identity);
        }

        private static UserManager<ApplicationUser> CreateUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();

            return new UserManager<ApplicationUser>(
                store.Object,
                null!,
                new PasswordHasher<ApplicationUser>(),
                [],
                [],
                null!,
                [],
                [],
                null!);
        }

        private static ShortUrlController CreateController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> manager,
            ClaimsPrincipal user)
        {
            var controller = new ShortUrlController(
                context,
                manager,
                null!);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };

            return controller;
        }

        [Fact]
        public async Task DeleteShortUrl_ReturnsOk_WhenUserDeletesOwnUrl()
        {
            
            var context = CreateInMemoryContext();

            var userId = Guid.NewGuid();

            var url = new ShortUrl
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://example.com",
                ShortCode = "abc123",
                CreatedById = userId,
                CreatedByUserName = "alice",
                CreatedDate = DateTime.UtcNow
            };

            context.ShortUrls.Add(url);
            await context.SaveChangesAsync();

            var manager = CreateUserManager();

            var controller = CreateController(
                context,
                manager,
                CreateUser(userId));

            var result = await controller.DeleteShortUrl(
                url.Id,
                CancellationToken.None);

            result.Should().BeOfType<OkResult>();

            var deletedUrl = await context.ShortUrls
                .FirstOrDefaultAsync(x => x.Id == url.Id);

            deletedUrl.Should().BeNull();
        }

        [Fact]
        public async Task DeleteShortUrl_ReturnsOk_WhenAdminDeletesAnotherUsersUrl()
        {
            
            var context = CreateInMemoryContext();

            var ownerId = Guid.NewGuid();
            var adminId = Guid.NewGuid();

            var url = new ShortUrl
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://example.com",
                ShortCode = "abc123",
                CreatedById = ownerId,
                CreatedByUserName = "alice",
                CreatedDate = DateTime.UtcNow
            };

            context.ShortUrls.Add(url);
            await context.SaveChangesAsync();

            var manager = CreateUserManager();

            var controller = CreateController(
                context,
                manager,
                CreateUser(adminId, isAdmin: true));

            
            var result = await controller.DeleteShortUrl(
                url.Id,
                CancellationToken.None);

            result.Should().BeOfType<OkResult>();

            var deletedUrl = await context.ShortUrls
                .FirstOrDefaultAsync(x => x.Id == url.Id);

            deletedUrl.Should().BeNull();
        }

        [Fact]
        public async Task DeleteShortUrl_ReturnsForbid_WhenUserDeletesAnotherUsersUrl()
        {
            
            var context = CreateInMemoryContext();

            var ownerId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();

            var url = new ShortUrl
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://example.com",
                ShortCode = "abc123",
                CreatedById = ownerId,
                CreatedByUserName = "alice",
                CreatedDate = DateTime.UtcNow
            };

            context.ShortUrls.Add(url);
            await context.SaveChangesAsync();

            var manager = CreateUserManager();

            var controller = CreateController(
                context,
                manager,
                CreateUser(anotherUserId));

            
            var result = await controller.DeleteShortUrl(
                url.Id,
                CancellationToken.None);

            result.Should().BeOfType<ForbidResult>();

            var existingUrl = await context.ShortUrls
                .FirstOrDefaultAsync(x => x.Id == url.Id);

            existingUrl.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteShortUrl_ReturnsNotFound_WhenUrlDoesNotExist()
        {
            
            var context = CreateInMemoryContext();

            var userId = Guid.NewGuid();

            var manager = CreateUserManager();

            var controller = CreateController(
                context,
                manager,
                CreateUser(userId));

            var urlId = Guid.NewGuid();

            var result = await controller.DeleteShortUrl(
                urlId,
                CancellationToken.None);

            result.Should().BeOfType<NotFoundResult>();

            (await context.ShortUrls.CountAsync())
                .Should().Be(0);
        }

        [Fact]
        public async Task DeleteShortUrl_ReturnsUnauthorized_WhenUserIdIsInvalid()
        {
            var context = CreateInMemoryContext();

            var manager = CreateUserManager();

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    "invalid-guid")
            };

            var identity = new ClaimsIdentity(
                claims,
                "TestAuthentication");

            var user = new ClaimsPrincipal(identity);

            var controller = CreateController(
                context,
                manager,
                user);

            var result = await controller.DeleteShortUrl(
                Guid.NewGuid(),
                CancellationToken.None);

            result.Should().BeOfType<UnauthorizedResult>();
        }
    }
}
