namespace Api.Models
{
    public class AboutViewModel
    {
        public string Content { get; set; } = string.Empty;
        public DateTime UpdatedDate { get; set; }
        public bool IsAdmin { get; set; }
    }
}
