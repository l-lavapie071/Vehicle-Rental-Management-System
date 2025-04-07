using Vehicle_Rental_Management_System.Data;
using Vehicle_Rental_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Rental_Management_System.Controllers
{

    [Authorize]
    [Route("[controller]")]
    public class ReservationController : Controller
    {
        private readonly App_Dbcontext _context;

        public ReservationController(App_Dbcontext context)
        {
            _context = context;
        }

        // Display all Reservation
        [HttpGet]
        public async Task<IActionResult> ReservationList()
        {
            var reservations = await _context.Reservations
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .ToListAsync();
            return View(reservations);
        }
    }
}
