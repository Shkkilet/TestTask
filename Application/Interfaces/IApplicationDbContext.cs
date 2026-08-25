using Microsoft.EntityFrameworkCore;

namespace Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Domain.Entities.ShortUrl> ShortUrls { get; }
        DbSet<Domain.Entities.AboutPage> AboutPages { get; }
        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
