using Application.Interfaces;
using Application.ShortUrl.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.ShortUrl.Queries.GetShortUrlById
{
    public class GetShortUrlByIdQueryHandler: IRequestHandler<GetShortUrlByIdQuery, ShortUrlDto>
    {
        private readonly IApplicationDbContext _context;
        public GetShortUrlByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<ShortUrlDto> Handle(GetShortUrlByIdQuery request, CancellationToken cancellationToken)
        {
            var url = await _context.ShortUrls.FirstOrDefaultAsync(x => x.Id == request.id);
            var result = new ShortUrlDto
            {
                Id = url.Id,
                OriginalUrl = url.OriginalUrl,
                ShortCode = url.ShortCode,
                ShortUrl = $"/s/{url.ShortCode}",
                CreatedByUserName = url.CreatedByUserName,
                CreatedById = url.CreatedById,
                CreatedDate = url.CreatedDate

            };
            return result;
        }
    }
}
