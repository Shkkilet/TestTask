namespace Application.ShortUrl.DTOs
{
    public class ShortUrlDto
    {
        public Guid Id { get; set; }
        public string OriginalUrl { get; set; } = null!;
        public string ShortCode { get; set; } = null!;
        public string ShortUrl { get; set; } = null!;
        public string CreatedByUserName { get; set; } = string.Empty;
        public Guid? CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool CanDelete { get; set; }
    }
}
