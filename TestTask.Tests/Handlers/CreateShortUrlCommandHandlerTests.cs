using Application.Interfaces;
using Application.Services;
using Application.ShortUrl.Commands.CreateShortUrlCommand;
using Application.ShortUrl.DTOs;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TestTask.Tests.Handlers
{
    public class CreateShortUrlCommandHandlerTests
    {
        private static IApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_CreatesShortUrl_WhenOriginalUrlIsUnique()
        {
            var context = CreateInMemoryContext();
            var generator = new ShortCodeGenerator(context);
            var sut = new CreateShortUrlCommandHandler(context, generator);

            var userId = Guid.NewGuid();
            var command = new CreateShortUrlCommand(
                new CreateShortUrlDto { OriginalUrl = "https://example.com/one" },
                userId,
                "alice");

            var result = await sut.Handle(command, CancellationToken.None);

            result.OriginalUrl.Should().Be("https://example.com/one");
            result.ShortCode.Should().HaveLength(6);
            result.ShortUrl.Should().Be($"/s/{result.ShortCode}");
            result.CreatedByUserName.Should().Be("alice");
            result.CanDelete.Should().BeTrue();

            (await context.ShortUrls.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task Handle_ThrowsInvalidOperationException_WhenUrlAlreadyExists()
        {
            var context = CreateInMemoryContext();
            var generator = new ShortCodeGenerator(context);
            var sut = new CreateShortUrlCommandHandler(context, generator);

            var firstCommand = new CreateShortUrlCommand(
                new CreateShortUrlDto { OriginalUrl = "https://example.com/dup" },
                Guid.NewGuid(),
                "alice");
            await sut.Handle(firstCommand, CancellationToken.None);

            var secondCommand = new CreateShortUrlCommand(
                new CreateShortUrlDto { OriginalUrl = "https://example.com/dup" },
                Guid.NewGuid(),
                "bob");

            var act = async () => await sut.Handle(secondCommand, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Short URL already exists for the given original URL");

            (await context.ShortUrls.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task Handle_GeneratesDifferentShortCodesForDifferentUrls()
        {
            var context = CreateInMemoryContext();
            var generator = new ShortCodeGenerator(context);
            var sut = new CreateShortUrlCommandHandler(context, generator);

            var first = await sut.Handle(
                new CreateShortUrlCommand(new CreateShortUrlDto { OriginalUrl = "https://example.com/a" }, Guid.NewGuid(), "alice"),
                CancellationToken.None);

            var second = await sut.Handle(
                new CreateShortUrlCommand(new CreateShortUrlDto { OriginalUrl = "https://example.com/b" }, Guid.NewGuid(), "bob"),
                CancellationToken.None);

            first.ShortCode.Should().NotBe(second.ShortCode);
        }

        [Fact]
        public async Task Handle_PersistsCreatedById_MatchingTheRequestingUser()
        {
            var context = CreateInMemoryContext();
            var generator = new ShortCodeGenerator(context);
            var sut = new CreateShortUrlCommandHandler(context, generator);
            var userId = Guid.NewGuid();

            await sut.Handle(
                new CreateShortUrlCommand(new CreateShortUrlDto { OriginalUrl = "https://example.com/owner" }, userId, "alice"),
                CancellationToken.None);

            var saved = await context.ShortUrls.SingleAsync();
            saved.CreatedById.Should().Be(userId);
        }
    }
}