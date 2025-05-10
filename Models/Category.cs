using System.ComponentModel.DataAnnotations;

namespace SesliKitapWeb.Models
{

    
    public class Category
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

        
    }

    
} 