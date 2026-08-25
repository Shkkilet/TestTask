using Application.Interfaces;
using Application.Services;
using Application.ShortUrl.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ShortUrl.Commands.CreateShortUrlCommand
{
    public class CreateShortUrlCommandHandler : IRequestHandler<CreateShortUrlCommand, ShortUrlDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ShortCodeGenerator _generator;
        public CreateShortUrlCommandHandler(IApplicationDbContext context, ShortCodeGenerator generator)
        {
            _context = context;
            _generator = generator;
        }

        public async Task<ShortUrlDto> Handle(CreateShortUrlCommand request, CancellationToken cancellationToken)
        {
            var exists = await _context.ShortUrls.AnyAsync(x => x.OriginalUrl == request.dto.OriginalUrl, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException("Short URL already exists for the given original URL");
            }

            string shortCode = await _generator.GenerateAsync();

            var shortUrl = new Domain.Entities.ShortUrl
            {

                Id = Guid.NewGuid(),
                OriginalUrl = request.dto.OriginalUrl,
                ShortCode = shortCode,
                CreatedById = request.userId,
                CreatedByUserName = request.userName,
                CreatedDate = DateTime.UtcNow
            };

            _context.ShortUrls.Add(shortUrl);

            await _context.SaveChangesAsync(cancellationToken);

            var result = new ShortUrlDto
            {
                Id = shortUrl.Id,
                OriginalUrl = shortUrl.OriginalUrl,
                ShortCode = shortUrl.ShortCode,
                ShortUrl = $"/s/{shortUrl.ShortCode}",
                CreatedByUserName = shortUrl.CreatedByUserName,
                CreatedDate = shortUrl.CreatedDate,
                CanDelete = shortUrl.CreatedById == request.userId,
            };
            return result;
        }

    }
}
