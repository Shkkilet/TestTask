using Application.Interfaces;
using Application.Services;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TestTask.Tests.Services
{
    public class ShortCodeGeneratorTests
    {
        private static IApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GenerateAsync_ReturnsCodeOfRequestedLength()
        {
            var context = CreateInMemoryContext();
            var sut = new ShortCodeGenerator(context);

            var code = await sut.GenerateAsync(length: 6);

            code.Should().HaveLength(6);
        }

        [Fact]
        public async Task GenerateAsync_ReturnsOnlyAllowedCharacters()
        {
            var context = CreateInMemoryContext();
            var sut = new ShortCodeGenerator(context);
            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var code = await sut.GenerateAsync();

            code.ToCharArray().Should().OnlyContain(c => allowedChars.Contains(c));
        }

        [Fact]
        public async Task GenerateAsync_GeneratesDifferentCodesAcrossCalls()
        {
            var context = CreateInMemoryContext();
            var sut = new ShortCodeGenerator(context);

            var codes = new List<string>();
            for (var i = 0; i < 20; i++)
            {
                codes.Add(await sut.GenerateAsync());
            }

            codes.Distinct().Should().HaveCount(codes.Count);
        }

        [Fact]
        public async Task GenerateAsync_SkipsCodesThatAlreadyExistInDatabase()
        {
            var context = CreateInMemoryContext();

            context.ShortUrls.Add(new Domain.Entities.ShortUrl
            {
                Id = Guid.NewGuid(),
                OriginalUrl = "https://example.com/seed",
                ShortCode = "AAAAAA",
                CreatedById = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            var sut = new ShortCodeGenerator(context);

            for (var i = 0; i < 50; i++)
            {
                var code = await sut.GenerateAsync();
                code.Should().NotBe("AAAAAA");
            }
        }
    }
}