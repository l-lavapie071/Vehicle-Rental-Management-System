using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Vehicle_Rental_Management_System.Models.ViewModels
{
    public class VehicleFormVM
    {
        [Required]
        public string Make { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        [Range(1900, int.MaxValue)]
        public int Year { get; set; }

        [Required]
        public string LicensePlate { get; set; }

        [Required]
        [Range(0, 1000000)]
        public int Mileage { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double RentalPricePerDay { get; set; }

        public string? Description { get; set; }

        public bool IsAvailable { get; set; }

        public IFormFile? ImageFile { get; set; }  // Uploaded image
    }
}
