namespace Domain.Entities
{
    public class AboutPage
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime UpdatedDate { get; set; }
    }
}
