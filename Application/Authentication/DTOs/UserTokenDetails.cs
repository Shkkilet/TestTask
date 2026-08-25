namespace Application.Authentication.DTOs
{
    public class UserTokenDetails
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public Guid CustomerId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
