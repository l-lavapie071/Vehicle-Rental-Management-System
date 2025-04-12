using Microsoft.AspNetCore.Mvc.Rendering;
using Vehicle_Rental_Management_System.Models;

namespace Vehicle_Rental_Management_System.Models.ViewModels
{
    public class ReservationFormVM
    {
        public Reservation Reservation { get; set; } = new Reservation();
        public List<SelectListItem>? Customers { get; set; } = new();
        public List<SelectListItem>? Vehicles { get; set; } = new();
        public decimal? NewMileage { get; set; }
    }
}
