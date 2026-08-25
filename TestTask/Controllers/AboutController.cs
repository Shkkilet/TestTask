using Api.Models;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    public class AboutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AboutController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            var about = await _context.AboutPages
                .FirstOrDefaultAsync(cancellationToken);

            if (about is null)
            {
                about = new AboutPage
                {
                    Content = GetDefaultContent(),
                    UpdatedDate = DateTime.UtcNow
                };

                _context.AboutPages.Add(about);

                await _context.SaveChangesAsync(cancellationToken);
            }

            var model = new AboutViewModel
            {
                Content = about.Content,
                UpdatedDate = about.UpdatedDate,
                IsAdmin = User.IsInRole("Admin")
            };

            return View(model);
        }

        [Authorize (AuthenticationSchemes = "MvcCookie", Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            string content,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError(
                    "content",
                    "Description cannot be empty.");
            }

            if (!ModelState.IsValid)
            {
                var about = await _context.AboutPages
                    .FirstOrDefaultAsync(cancellationToken);

                var model = new AboutViewModel
                {
                    Content = content,
                    UpdatedDate = about?.UpdatedDate ?? DateTime.UtcNow,
                    IsAdmin = true
                };

                return View(model);
            }

            var page = await _context.AboutPages
                .FirstOrDefaultAsync(cancellationToken);

            if (page is null)
            {
                page = new AboutPage();

                _context.AboutPages.Add(page);
            }

            page.Content = content;
            page.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return RedirectToAction(nameof(Index));
        }

        private static string GetDefaultContent()
        {
            return """
                URL Shortener Algorithm

                The application generates a unique short code for every original URL.

                When a user creates a short URL, the application first checks whether
                the original URL already exists in the database. If it already exists,
                the user receives an error because original URLs must be unique.

                For a new URL, the application generates a random short code using
                lowercase letters, uppercase letters and digits.

                The generated code is checked against the database to make sure that
                another short URL does not already use the same code. If the code
                already exists, another code is generated.

                Once a unique code has been generated, the original URL, short code,
                creator and creation date are stored in the database.

                When someone opens the short URL, the application finds the
                corresponding record by its short code and redirects the user to
                the original URL.
                """;
        }
    }
}