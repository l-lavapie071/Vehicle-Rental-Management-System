using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Rental_Management_System.Data;
using Vehicle_Rental_Management_System.Models;

namespace Vehicle_Rental_Management_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly App_Dbcontext _context;

        public HomeController(App_Dbcontext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("home")]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("", "App");
            }

            var reservations = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                //.Where(r => r.IsReturned)
                .ToListAsync();

            var totalRevenue = reservations.Sum(r =>
                (r.EndDate - r.StartDate).Days * (r.Vehicle?.RentalPricePerDay ?? 0));

            var taxRate = 0.05;
            var taxCollected = totalRevenue * taxRate;

            var mostRentedVehicles = reservations
                .Where(r => r.Vehicle != null) // Ensure Vehicle is not null
                .GroupBy(r => r.VehicleId)
                .Select(g => new
                {
                    Vehicle = g.First().Vehicle,
                    TimesRented = g.Count(),
                    Revenue = g.Sum(r => (r.EndDate - r.StartDate).Days * (r.Vehicle?.RentalPricePerDay ?? 0))
                })
                .OrderByDescending(v => v.TimesRented)
                .ThenByDescending(v => v.Revenue)
                .Take(5)
                .ToList();


            var customerActivity = reservations
                .GroupBy(r => r.CustomerId)
                .Select(g => new
                {
                    Customer = g.First().Customer,
                    Reservations = g.Count(),
                    TotalSpent = g.Sum(r => (r.EndDate - r.StartDate).Days * (r.Vehicle?.RentalPricePerDay ?? 0))
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(5) // Only take top 5
                .ToList();
            if (reservations.Any())
            {
                ViewBag.TotalRevenue = totalRevenue;
                ViewBag.TaxCollected = taxCollected;
                ViewBag.TotalReservations = reservations.Count;
                ViewBag.AverageRentalDuration = reservations.Average(r => (r.EndDate - r.StartDate).TotalDays);
                ViewBag.MostRentedVehicles = mostRentedVehicles;
                ViewBag.CustomerActivity = customerActivity;
                ViewBag.Reservations = reservations;
            }
            

            return View();
        }



    }


}
