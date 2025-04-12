using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        public Customer? Customer { get; set; }

        public int ReservationId { get; set; }

        [ForeignKey("ReservationId")]
        public Reservation? Reservation { get; set; }

        [Column(TypeName = "decimal(18, 2)")] 
        public double Tax { get; set; } = 0.05; // Default value set to 5%
        [Column(TypeName = "decimal(18, 2)")]
        public double InsuranceFee { get; set; } = 30; // Default insurance fee

        [Column(TypeName = "decimal(18, 2)")]
        public double CleaningFee { get; set; } = 25; // Default cleaning fee

        public double TotalAmount { get; set; }

        public DateTime BillingDate { get; set; }

        public Billing()
        {
            Tax = 0.05; // Default value set to 5%
            InsuranceFee = 30;
            CleaningFee = 25;
        }
    }
}