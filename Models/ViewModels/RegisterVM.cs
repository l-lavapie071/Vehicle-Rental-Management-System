using System.ComponentModel.DataAnnotations;
namespace Vehicle_Rental_Management_System.Models.ViewModels
{

    public class RegisterVM
    {
        [Required(ErrorMessage = "Username is required.")]
        public string? Username { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "First Name is required.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        public string? LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Email is required.")]
        public string? Email { get; set; }

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "Phone Number is required.")]
        public string? PhoneNumber { get; set; }

        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Address is required.")]
        public string? Address { get; set; }


    }
}
