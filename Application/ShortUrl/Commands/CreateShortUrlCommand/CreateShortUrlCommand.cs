using Application.ShortUrl.DTOs;
using MediatR;

namespace Application.ShortUrl.Commands.CreateShortUrlCommand
{
    public record CreateShortUrlCommand(CreateShortUrlDto dto, Guid userId, string userName): IRequest<ShortUrlDto>;
}
