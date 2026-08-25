using Application.ShortUrl.DTOs;
using MediatR;

namespace Application.ShortUrl.Queries.GetShortUrlById
{
    public record GetShortUrlByIdQuery(Guid id) : IRequest<ShortUrlDto>;
}
