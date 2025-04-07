using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Rental_Management_System.Models
{
    public class Billing
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        public int ReservationId { get; set; }

        [ForeignKey("ReservationId")]
        public Reservation Reservation { get; set; }

        [Column(TypeName = "decimal(18, 2)")] // Specify column type directly in the model
        public decimal Tax { get; set; } = 0.05m; // Default value set to 5%

        public decimal TotalAmount { get; set; }

        public DateOnly BillingDate { get; set; }

        public Billing()
        {
            Tax = 0.05m; // Default value set to 5%
        }
    }
}