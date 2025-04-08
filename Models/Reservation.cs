using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Rental_Management_System.Models
{
    public class Reservation
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }


        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsReturned { get; set; }
    }
}