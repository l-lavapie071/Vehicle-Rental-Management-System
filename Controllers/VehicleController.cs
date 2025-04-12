using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using Vehicle_Rental_Management_System.Data;
using Vehicle_Rental_Management_System.Models;
using Vehicle_Rental_Management_System.Models.ViewModels;

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
        // Display all Vehicle with optional filtering
        [HttpGet]
        public async Task<IActionResult> VehicleList(string type, string make, string model, bool? availability)
        {
            // Start with all vehicles
            var vehicles = await _context.Vehicles.ToListAsync();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(type))
            {
                vehicles = vehicles.Where(v => v.Type.Contains(type, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(make))
            {
                vehicles = vehicles.Where(v => v.Make.Contains(make, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrEmpty(model))
            {
                vehicles = vehicles.Where(v => v.Model.Contains(model, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (availability.HasValue)
            {
                vehicles = vehicles.Where(v => v.IsAvailable == availability.Value).ToList();
            }

            // Return the filtered list to the view
            return View(vehicles);
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
        public async Task<IActionResult> AddVehicle(VehicleFormVM vm)
        {
            //for debugging
            //var make = vm.Make;
            //var model = vm.Model;
            //var year = vm.Year;
            //var plate = vm.LicensePlate;
            //var price = vm.RentalPricePerDay;
            //var mileage = vm.Mileage;
            //var availability = vm.IsAvailable;

            if (ModelState.IsValid)
            {
                string imagePath = string.Empty;

                if (vm.ImageFile != null)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    Directory.CreateDirectory(uploadsFolder); // ensure folder exists

                    //format file name as make_model.jpg
                    var makeModelName = $"{vm.Make}_{vm.Model}_{vm.LicensePlate}".ToLower().Replace(" ", "_");
                    var extension = Path.GetExtension(vm.ImageFile.FileName);
                    var fileName = $"{makeModelName}{extension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await vm.ImageFile.CopyToAsync(fileStream);
                    }

                    imagePath = $"/images/{fileName}";
                }

                var vehicle = new Vehicle
                {
                    Make = vm.Make,
                    Model = vm.Model,
                    Type = vm.Type,
                    Year = vm.Year,
                    LicensePlate = vm.LicensePlate.ToUpper(),
                    Mileage = vm.Mileage,
                    RentalPricePerDay = vm.RentalPricePerDay,
                    Description = vm.Description,
                    IsAvailable = true,//default value set to true
                    ImageUrl = imagePath
                };

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(VehicleList));
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }
            //return View(model);

            return View(vm);
        }
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> EditVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }
            return View(vehicle);
        }


        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, Vehicle vehicle, IFormFile? Image)
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            // Query the existing vehicle from the database
            var existingVehicle = await _context.Vehicles.FindAsync(id);
            if (existingVehicle == null)
            {
                return NotFound();
            }

            // Update properties from the form, except the ImageUrl
            if (ModelState.IsValid)
            {
                try
                {
                    if (Image != null)
                    {
                        // Define the folder path where images are stored
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

                        // Format file name as make_model_licenseplate.jpg
                        var makeModelName = $"{vehicle.Make}_{vehicle.Model}_{vehicle.LicensePlate}".ToLower().Replace(" ", "_");
                        var extension = Path.GetExtension(Image.FileName);
                        var fileName = $"{makeModelName}{extension}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Check if the file already exists and delete it
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath); // Delete the old image
                        }

                        // Save the new image
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await Image.CopyToAsync(fileStream); // Correct variable name is 'Image'
                        }

                        // Set the new image path
                        existingVehicle.ImageUrl = $"/images/{fileName}";
                    }

                    // Update other fields from the form to the existing vehicle object
                    existingVehicle.Make = vehicle.Make;
                    existingVehicle.Model = vehicle.Model;
                    existingVehicle.Type = vehicle.Type;
                    existingVehicle.Year = vehicle.Year;
                    existingVehicle.LicensePlate = vehicle.LicensePlate;
                    existingVehicle.Mileage = vehicle.Mileage;
                    existingVehicle.RentalPricePerDay = vehicle.RentalPricePerDay;
                    existingVehicle.Description = vehicle.Description;
                    existingVehicle.IsAvailable = vehicle.IsAvailable;

                    // Update the vehicle record in the database
                    _context.Update(existingVehicle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Vehicles.Any(v => v.Id == vehicle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(VehicleDetails), new { id = existingVehicle.Id });
            }
            return View(vehicle); 
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            //delete image file
            if (!string.IsNullOrEmpty(vehicle.ImageUrl))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, vehicle.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(VehicleList));
        }




    }
}
