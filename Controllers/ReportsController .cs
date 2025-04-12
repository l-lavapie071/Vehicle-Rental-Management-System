using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using Vehicle_Rental_Management_System.Data;
using Vehicle_Rental_Management_System.Models;
using System.Text;


namespace Vehicle_Rental_Management_System.Controllers
{
    public class ReportsController : Controller
    {
        private readonly App_Dbcontext _context;

        public ReportsController(App_Dbcontext context)
        {
            _context = context;
        }

        // Report Generation with Filters
        public async Task<IActionResult> FilteredRentalRevenueReport(DateTime? startDate, DateTime? endDate, string vehicleType, string customerName)
        {
            var reservationsQuery = _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .AsQueryable();

            // Filter by date range if provided
            if (startDate.HasValue)
                reservationsQuery = reservationsQuery.Where(r => r.StartDate >= startDate.Value);

            if (endDate.HasValue)
                reservationsQuery = reservationsQuery.Where(r => r.EndDate <= endDate.Value);

            // Filter by vehicle type if provided
            if (!string.IsNullOrEmpty(vehicleType))
                reservationsQuery = reservationsQuery.Where(r => r.Vehicle.Type == vehicleType);

            // Filter by customer name if provided
            if (!string.IsNullOrEmpty(customerName))
                reservationsQuery = reservationsQuery.Where(r => r.Customer.FirstName.Contains(customerName) || r.Customer.LastName.Contains(customerName));

            var reservations = await reservationsQuery.ToListAsync();

            // Calculate revenue and other stats
            var totalRevenue = reservations.Sum(r =>
                (r.EndDate - r.StartDate).Days * (r.Vehicle?.RentalPricePerDay ?? 0));

            var taxRate = 0.05;
            var taxCollected = totalRevenue * taxRate;

            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TaxCollected = taxCollected;
            ViewBag.Reservations = reservations;

            return View();
        }
        public IActionResult DownloadCsvReport(DateTime? startDate, DateTime? endDate, string vehicleType, string customerName)
        {
            var reservations = _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .Where(r => (!startDate.HasValue || r.StartDate >= startDate)
                         && (!endDate.HasValue || r.EndDate <= endDate)
                         && (string.IsNullOrEmpty(vehicleType) || r.Vehicle.Type.Contains(vehicleType))
                         && (string.IsNullOrEmpty(customerName) ||
                             (r.Customer.FirstName + " " + r.Customer.LastName).Contains(customerName)))
                .ToList();

            var csv = new StringBuilder();
            csv.AppendLine("Customer,Vehicle,Type,Start Date,End Date,Total Cost");

            foreach (var r in reservations)
            {
                var days = (r.EndDate - r.StartDate).Days;
                var cost = days * (r.Vehicle?.RentalPricePerDay ?? 0);

                string customerNameSafe = EscapeCsv($"{r.Customer.FirstName} {r.Customer.LastName}");
                string vehicleSafe = EscapeCsv($"{r.Vehicle.Make} {r.Vehicle.Model}");
                string typeSafe = EscapeCsv(r.Vehicle.Type);
                string costSafe = EscapeCsv(cost.ToString("C2"));

                csv.AppendLine($"{customerNameSafe},{vehicleSafe},{typeSafe},{r.StartDate:yyyy-MM-dd},{r.EndDate:yyyy-MM-dd},{costSafe}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", "FilteredRevenueReport.csv");
        }

        // Helper to safely wrap/escape CSV values
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            value = value.Replace("\"", "\"\""); // Escape quotes
            return $"\"{value}\"";               // Wrap in quotes
        }

    }

}
