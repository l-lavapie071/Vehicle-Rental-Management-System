using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using Vehicle_Rental_Management_System.Data;
using Vehicle_Rental_Management_System.Models;

namespace Vehicle_Rental_Management_System.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class VehicleController : Controller
    {
        private readonly App_Dbcontext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public VehicleController(App_Dbcontext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        // Display all Vehicle
        [HttpGet]
        public async Task<IActionResult> VehicleList()
        {
            var vehiles = await _context.Vehicles.ToListAsync();
            return View(vehiles);
        }

        // Display Vehicle details
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> VehicleDetails(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            return vehicle != null ? View(vehicle) : NotFound();
        }

        // Show create form
        [HttpGet("AddVehicle")]
        public IActionResult AddVehicle()
        {
            return View();
        }

        // Add a new Vehicle
        [HttpPost("AddVehicle")]
        public async Task<IActionResult> AddVehicle(Vehicle newVehicle, IFormFile ImageFile)
        {
            var make = newVehicle.Make;
            var model = newVehicle.Model;
            var year = newVehicle.Year;
            var plate = newVehicle.LicensePlate;
            var price = newVehicle.RentalPricePerDay;
            var mileage = newVehicle.Mileage;
            var availability = newVehicle.IsAvailable;
            var filename = ImageFile.FileName;

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadsFolder); // ensure folder exists

                    //format file name as make_model.jpg
                    var makeModelName = $"{newVehicle.Make}_{newVehicle.Model}".ToLower().Replace(" ", "_");
                    var extension = Path.GetExtension(ImageFile.FileName);
                    var fileName = $"{makeModelName}{extension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    newVehicle.ImageUrl = "/images/" + fileName;
                }

                _context.Vehicles.Add(newVehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(VehicleList));
           }
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }
            //return View(model);

            return View(newVehicle);
        }
    }
}
