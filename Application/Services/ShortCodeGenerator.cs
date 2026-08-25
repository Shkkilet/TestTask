using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class ShortCodeGenerator
    {
        private readonly IApplicationDbContext _context;
        private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public ShortCodeGenerator(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<string> GenerateAsync(int length = 6)
        {
            var codeChars = new char[length];

            while (true)
            {
                for (var i = 0; i < length; i++)
                {
                    var randomIndex = Random.Shared.Next(Characters.Length);

                    codeChars[i] = Characters[randomIndex];
                }
            
                var code = new string(codeChars);

                if(! await _context.ShortUrls.AnyAsync(u => u.ShortCode == code))
                { return code; }

            }
        }
    }
}
