using Application.Authentication.DTOs;
using MediatR;

namespace Application.Authentication.Commands.LoginCommand
{
    public record LoginCommand() : IRequest<AuthResponse>;
}
