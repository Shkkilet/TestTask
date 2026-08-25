using Application.ShortUrl.Commands.CreateShortUrlCommand;
using Application.ShortUrl.DTOs;
using Application.ShortUrl.Queries.GetAllShortUrls;
using Application.ShortUrl.Queries.GetShortUrlById;
using Infrastructure.Data;
using Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShortUrlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _manager;
        private readonly ISender _sender;
        public ShortUrlController(ApplicationDbContext context, UserManager<ApplicationUser> manager, ISender sender)
        {
            _context = context;
            _manager = manager;
            _sender = sender;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateShortUrl([FromBody] CreateShortUrlDto dto, CancellationToken cancellationToken)
        {
            var userId = _manager.GetUserId(User);
            var userName = _manager.GetUserName(User);
            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var request = new CreateShortUrlCommand(dto, parsedUserId, userName);

            var result = await _sender.Send(request, cancellationToken);

            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetShortUrls(CancellationToken cancellationToken)
        {
            var userId = _manager.GetUserId(User);

            var guidUserId = Guid.TryParse(userId, out var parsedGuid) ? parsedGuid : Guid.Empty;

            var request = new GetAllShortUrlsQuery(guidUserId);

            var result = await _sender.Send(request, cancellationToken);
            return Ok(result);
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetShortUrlById(Guid id, CancellationToken cancellationToken)
        {
            var request = new GetShortUrlByIdQuery(id);

            var result = await _sender.Send(request, cancellationToken);
            return Ok(result);
        }
        [HttpGet("/s/{shortCode}")]
        [AllowAnonymous]
        public async Task<IActionResult> RedirectToOriginal(string shortCode, CancellationToken cancellationToken)
        {
            var url = await _context.ShortUrls.FirstOrDefaultAsync(x => x.ShortCode == shortCode, cancellationToken);
            if (url is null) return NotFound();

            return Redirect(url.OriginalUrl);
        }
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteShortUrl(Guid id, CancellationToken cancellationToken)
        {
            var userId = _manager.GetUserId(User);
            
            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }
            var isAdmin = User.IsInRole("Admin");
            var url = await _context.ShortUrls.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (url is null) return NotFound();

            if(parsedUserId != url.CreatedById && !isAdmin)
            {
                return Forbid();
            }
            _context.ShortUrls.Remove(url);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok();
        }
    }
}
