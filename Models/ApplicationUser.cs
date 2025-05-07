using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SesliKitapWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsDarkMode { get; set; }
        public virtual ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
} 