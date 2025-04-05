using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Vehicle_Rental_Management_System.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Address { get; set; }
    }
}
