namespace Domain.Entities
{
    public class ShortUrl
    {
        public Guid Id { get; set; }
        public string OriginalUrl { get; set; } = null!;
        public string ShortCode { get; set; } = null!;
        public Guid? CreatedById { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
