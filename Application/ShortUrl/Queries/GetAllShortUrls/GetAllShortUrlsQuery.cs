using Application.ShortUrl.DTOs;
using MediatR;

namespace Application.ShortUrl.Queries.GetAllShortUrls
{
    public record GetAllShortUrlsQuery(Guid? userId) : IRequest<List<ShortUrlDto>>;
}
