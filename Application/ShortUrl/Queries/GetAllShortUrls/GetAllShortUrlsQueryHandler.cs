using Application.Interfaces;
using Application.ShortUrl.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ShortUrl.Queries.GetAllShortUrls
{
    public class GetAllShortUrlsQueryHandler: IRequestHandler<GetAllShortUrlsQuery, List<ShortUrlDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllShortUrlsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShortUrlDto>> Handle(GetAllShortUrlsQuery request, CancellationToken cancellationToken)
        {
            var urls = await _context.ShortUrls
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync(cancellationToken);

            var result = urls.Select(x => new ShortUrlDto
            {
                Id = x.Id,
                OriginalUrl = x.OriginalUrl,
                ShortCode = x.ShortCode,
                ShortUrl = $"/s/{x.ShortCode}",
                CreatedByUserName = x.CreatedByUserName,
                CreatedById = x.CreatedById,
                CreatedDate = x.CreatedDate,
                CanDelete = x.CreatedById == request.userId,

            }).ToList();

            return result;
        }
    }
}
