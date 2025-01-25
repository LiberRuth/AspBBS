using System.ComponentModel.DataAnnotations;

namespace AspBBS.Models
{
    public class WriteModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Text is required.")]
        public string Text { get; set; } = string.Empty;

        public string IP { get; set; } = string.Empty;

        public string CreatedAt { get; set; } = string.Empty;

        public string Situation { get; set; } = string.Empty;
    }
}
