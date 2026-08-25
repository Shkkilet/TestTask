using Application.ShortUrl.Queries.GetAllShortUrls;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TestTask.Tests.Handlers
{
    public class GetAllShortUrlsQueryHandlerTests
    {
        private static ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ReturnsAllShortUrls()
        {
            var context = CreateInMemoryContext();

            var firstUserId = Guid.NewGuid();
            var secondUserId = Guid.NewGuid();

            context.ShortUrls.AddRange(
                new ShortUrl
                {
                    Id = Guid.NewGuid(),
                    OriginalUrl = "https://example.com/one",
                    ShortCode = "abc123",
                    CreatedById = firstUserId,
                    CreatedByUserName = "alice",
                    CreatedDate = DateTime.UtcNow.AddMinutes(-10)
                },
                new ShortUrl
                {
                    Id = Guid.NewGuid(),
                    OriginalUrl = "https://example.com/two",
                    ShortCode = "xyz789",
                    CreatedById = secondUserId,
                    CreatedByUserName = "bob",
                    CreatedDate = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var handler = new GetAllShortUrlsQueryHandler(context);

            var query = new GetAllShortUrlsQuery(firstUserId);

            var result = await handler.Handle(
                query,
                CancellationToken.None);

            result.Should().HaveCount(2);

            result.Should().Contain(x =>
                x.OriginalUrl == "https://example.com/one" &&
                x.ShortCode == "abc123" &&
                x.CreatedByUserName == "alice");

            result.Should().Contain(x =>
                x.OriginalUrl == "https://example.com/two" &&
                x.ShortCode == "xyz789" &&
                x.CreatedByUserName == "bob");
        }

        [Fact]
        public async Task Handle_SetsCanDeleteTrue_ForCurrentUsersUrl()
        {
            var context = CreateInMemoryContext();

            var currentUserId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();

            var ownUrl = new ShortUrl
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://example.com/own",
                ShortCode = "own123",
                CreatedById = currentUserId,
                CreatedByUserName = "alice",
                CreatedDate = DateTime.UtcNow
            };

            var anotherUsersUrl = new ShortUrl
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://example.com/other",
                ShortCode = "oth123",
                CreatedById = anotherUserId,
                CreatedByUserName = "bob",
                CreatedDate = DateTime.UtcNow
            };

            context.ShortUrls.AddRange(
                ownUrl,
                anotherUsersUrl);

            await context.SaveChangesAsync();

            var handler = new GetAllShortUrlsQueryHandler(context);

            var query = new GetAllShortUrlsQuery(currentUserId);

            var result = await handler.Handle(
                query,
                CancellationToken.None);

            result.Should().HaveCount(2);

            result.Single(x => x.Id == ownUrl.Id)
                .CanDelete
                .Should().BeTrue();

            result.Single(x => x.Id == anotherUsersUrl.Id)
                .CanDelete
                .Should().BeFalse();
        }

        [Fact]
        public async Task Handle_SetsCanDeleteFalse_WhenUserIsAnonymous()
        {
            var context = CreateInMemoryContext();

            context.ShortUrls.Add(
                new ShortUrl
                {
                    Id = Guid.NewGuid(),
                    OriginalUrl = "https://example.com/test",
                    ShortCode = "abc123",
                    CreatedById = Guid.NewGuid(),
                    CreatedByUserName = "alice",
                    CreatedDate = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var handler = new GetAllShortUrlsQueryHandler(context);

            var query = new GetAllShortUrlsQuery(null);

            var result = await handler.Handle(
                query,
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].CanDelete.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ReturnsUrlsOrderedByCreatedDateDescending()
        {
            var context = CreateInMemoryContext();

            var oldest = new ShortUrl
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://example.com/old",
                ShortCode = "old123",
                CreatedById = Guid.NewGuid(),
                CreatedByUserName = "alice",
                CreatedDate = DateTime.UtcNow.AddHours(-2)
            };

            var middle = new ShortUrl
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://example.com/middle",
                ShortCode = "mid123",
                CreatedById = Guid.NewGuid(),
                CreatedByUserName = "bob",
                CreatedDate = DateTime.UtcNow.AddHours(-1)
            };

            var newest = new ShortUrl
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://example.com/new",
                ShortCode = "new123",
                CreatedById = Guid.NewGuid(),
                CreatedByUserName = "charlie",
                CreatedDate = DateTime.UtcNow
            };

            context.ShortUrls.AddRange(
                oldest,
                middle,
                newest);

            await context.SaveChangesAsync();

            var handler = new GetAllShortUrlsQueryHandler(context);

            var query = new GetAllShortUrlsQuery(null);

            var result = await handler.Handle(
                query,
                CancellationToken.None);

            result.Select(x => x.ShortCode)
                .Should()
                .ContainInOrder(
                    "new123",
                    "mid123",
                    "old123");
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenNoShortUrlsExist()
        {
            var context = CreateInMemoryContext();

            var handler = new GetAllShortUrlsQueryHandler(context);

            var query = new GetAllShortUrlsQuery(null);

            var result = await handler.Handle(
                query,
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
