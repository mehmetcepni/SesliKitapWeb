using System.ComponentModel.DataAnnotations;

namespace SesliKitapWeb.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Author { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string CoverImageUrl { get; set; } = string.Empty;

        public string PdfFileUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? PdfFilePath { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();

        public string Category { get; set; } = string.Empty;
    }
}