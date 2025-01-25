using System.ComponentModel.DataAnnotations;

namespace AspBBS.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;

        public string IP { get; set; } = string.Empty;

        public string CreatedAt { get; set; } = string.Empty;
    }
}
