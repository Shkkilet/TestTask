using System.ComponentModel.DataAnnotations;

namespace Application.Authentication.DTOs
{
    public class RegisterDto
    {
        [property: Required]
        public string UserName { get; set; } = null!;
        [property: Required]
        [property: EmailAddress]
        public string Email { get; set; } = null!;

        [property: Required]
        public string Password { get; set; } = null!;
    }
}
