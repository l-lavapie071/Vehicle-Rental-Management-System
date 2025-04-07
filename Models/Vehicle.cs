using System.ComponentModel.DataAnnotations;

namespace Vehicle_Rental_Management_System.Models
{
    public class Vehicle
    {
        [Required]
        [Key]
        public int Id { get; set; }  // Unique Identifier for the vehicle

        [Required]
        [StringLength(100)]
        public string Make { get; set; }  // Vehicle Make 

        [Required]
        [StringLength(100)]
        public string Model { get; set; }  // Vehicle Model

        [Required]
        [Range(1900, int.MaxValue, ErrorMessage = "Year must be greater than or equal to 1900")]
        public int Year { get; set; }  // Manufacturing Year of the Vehicle

        [Required]
        [StringLength(20)]
        public string? LicensePlate { get; set; }  // License plate number

        [Required]
        [Range(0, 1000000)]  // Example: mileage can range from 0 to 1 million miles/km
        public int Mileage { get; set; }  // Mileage of the vehicle in miles or kilometers

        [Required]
        [Range(0, double.MaxValue)]
        public double RentalPricePerDay { get; set; }  // Daily rental price

        [StringLength(500)]
        public string? Description { get; set; }  // A description of the vehicle (optional)

        public bool IsAvailable { get; set; }  // Availability status for rental

        public string? ImageUrl { get; set; }  // Optional URL for vehicle image
    }
}

