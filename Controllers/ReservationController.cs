using Vehicle_Rental_Management_System.Data;
using Vehicle_Rental_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vehicle_Rental_Management_System.Models.ViewModels;

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
        public async Task<IActionResult> ReservationList(string customer, string make, string model, string returned)
        {
            var reservations = _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .AsQueryable();

            if (!string.IsNullOrEmpty(customer))
            {
                reservations = reservations.Where(r =>
                    (r.Customer.FirstName + " " + r.Customer.LastName).ToLower().Contains(customer.ToLower()));
            }

            if (!string.IsNullOrEmpty(make))
            {
                reservations = reservations.Where(r =>
                    r.Vehicle.Make.ToLower().Contains(make.ToLower()));
            }

            if (!string.IsNullOrEmpty(model))
            {
                reservations = reservations.Where(r =>
                    r.Vehicle.Model.ToLower().Contains(model.ToLower()));
            }

            if (!string.IsNullOrEmpty(returned))
            {
                bool isReturned = returned == "true";
                reservations = reservations.Where(r => r.IsReturned == isReturned);
            }

            var filteredList = await reservations.ToListAsync();
            return View(filteredList);
        }


        // Get Reservation Details
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> ReservationDetails(int id)
        {
            var reservation = await _context.Reservations
            .Where(r => r.Id == id)
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync();
            if (reservation == null)
                return NotFound();

            

            var bill = await _context.Billings
            .FirstOrDefaultAsync(b => b.ReservationId == reservation.Id);

            ViewBag.Bill = bill;

            return View(reservation);
        }

        // Show create form
        [HttpGet("AddReservation")]
        public IActionResult AddReservation()
        {
            var vm = new ReservationFormVM
            {
                Customers = _context.Customers
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.FirstName} {c.MiddleName} {c.LastName}"
            }).ToList(),

                Vehicles = _context.Vehicles
            .Where(v => v.IsAvailable) // filter only available ones
            .Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.Make} {v.Model} {v.Year} ({v.LicensePlate})"
            }).ToList()
            };

            return View(vm);
        }
        [HttpPost("AddReservation")]
        public IActionResult AddReservation(ReservationFormVM vm)
        {
            if (!ModelState.IsValid)
            {
                // Reload dropdowns if validation fails
                vm.Customers = _context.Customers
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.FirstName} {c.LastName}"
                    }).ToList();

                vm.Vehicles = _context.Vehicles
                    .Where(v => v.IsAvailable)
                    .Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text = $"{v.Make} {v.Model} ({v.LicensePlate})"
                    }).ToList();

                return View(vm);
            }

            // Find and validate the selected vehicle
            var vehicle = _context.Vehicles.FirstOrDefault(v => v.Id == vm.Reservation.VehicleId);

            if (vehicle == null || !vehicle.IsAvailable)
            {
                ModelState.AddModelError("", "Selected vehicle is not available.");

                // Repopulate dropdowns again for return view
                vm.Customers = _context.Customers
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.FirstName} {c.LastName}"
                    }).ToList();

                vm.Vehicles = _context.Vehicles
                    .Where(v => v.IsAvailable)
                    .Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text = $"{v.Make} {v.Model} ({v.LicensePlate})"
                    }).ToList();

                return View(vm);
            }

            // Set reservation flags
            vm.Reservation.IsReturned = false;

            // Save reservation and mark vehicle unavailable
            _context.Reservations.Add(vm.Reservation);
            vehicle.IsAvailable = false;

            _context.SaveChanges();

            return RedirectToAction("ReservationList");
        }
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> EditReservation(int id)
        {
            var reservation = await _context.Reservations
                                             .Include(r => r.Customer)
                                             .Include(r => r.Vehicle)
                                             .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            // Populate the ViewModel with necessary data
            var viewModel = new ReservationFormVM
            {
                Reservation = reservation,
                Customers = _context.Customers
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.FirstName} {c.LastName}"
                    }).ToList(),
                Vehicles = _context.Vehicles
                    .Where(v => v.IsAvailable || v.Id == reservation.VehicleId) // Filter available vehicles
                    .Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text = $"{v.Make} {v.Model} ({v.LicensePlate})"
                    })
                    .ToList()
            };

            return View(viewModel);
        }
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> EditReservation(int id, ReservationFormVM viewModel)
        {
            if (id != viewModel.Reservation.Id)
            {
                return NotFound();
            }

            // Check if 'IsReturned' is true and 'NewMileage' is null
            if (viewModel.Reservation.IsReturned && !viewModel.NewMileage.HasValue)
            {
                ModelState.AddModelError("NewMileage", "New Mileage is required when the vehicle is returned.");
            }

            // Validate that the new mileage is greater than the current mileage
            if (viewModel.NewMileage.HasValue)
            {
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == viewModel.Reservation.VehicleId);
                if (vehicle != null && viewModel.NewMileage < vehicle.Mileage)
                {
                    ModelState.AddModelError("NewMileage", "New Mileage must be greater than the prevoius mileage.");
                }
            }

            if (ModelState.IsValid)
            {
                var reservation = await _context.Reservations
                    .Include(r => r.Vehicle)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (reservation == null)
                {
                    return NotFound();
                }

                var oldVehicle = reservation.Vehicle;
                var newVehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == viewModel.Reservation.VehicleId);

                // Check if vehicle has been changed
                if (oldVehicle.Id != newVehicle.Id)
                {
                    // If the old vehicle is being returned, set it as available
                    if (reservation.IsReturned)
                    {
                        oldVehicle.IsAvailable = true;
                    }
                    else
                    {
                        oldVehicle.IsAvailable = false;
                    }

                    // Set the new vehicle as unavailable
                    newVehicle.IsAvailable = false;

                    // If the reservation is marked as returned, set the new vehicle as available
                    if (viewModel.Reservation.IsReturned)
                    {
                        newVehicle.IsAvailable = true;
                    }

                    // Update the vehicles in the database
                    _context.Update(oldVehicle);
                    _context.Update(newVehicle);
                }
                else
                {
                    // If the vehicle has not changed, update the availability of the old vehicle
                    if (viewModel.Reservation.IsReturned)
                    {
                        oldVehicle.IsAvailable = true;
                    }
                    else
                    {
                        oldVehicle.IsAvailable = false;
                    }

                    // Only update the old vehicle
                    _context.Update(oldVehicle);
                }

                // Update vehicle mileage
                if (viewModel.NewMileage.HasValue)
                {
                    oldVehicle.Mileage = (int)viewModel.NewMileage.Value; // Update the mileage of the old vehicle
                    _context.Update(oldVehicle);
                }

                // Update the reservation details
                reservation.CustomerId = viewModel.Reservation.CustomerId;
                reservation.VehicleId = viewModel.Reservation.VehicleId;
                reservation.StartDate = viewModel.Reservation.StartDate;
                reservation.EndDate = viewModel.Reservation.EndDate;
                reservation.IsReturned = viewModel.Reservation.IsReturned;

                // Update the reservation in the database
                _context.Update(reservation);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(ReservationList)); // Redirect to the reservation list
            }

            // Repopulate the SelectLists in case of validation errors
            viewModel.Customers = _context.Customers
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FirstName} {c.LastName}"
                }).ToList();

            viewModel.Vehicles = _context.Vehicles
                .Where(v => v.IsAvailable || v.Id == viewModel.Reservation.VehicleId) // Filter available vehicles
                .Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.Make} {v.Model} ({v.LicensePlate})"
                })
                .ToList();

            return View(viewModel);
        }

        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            var vehicle = await _context.Vehicles.FindAsync(reservation.VehicleId);
            if (vehicle != null)
            {
                vehicle.IsAvailable = true;
                _context.Vehicles.Update(vehicle);
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ReservationList));
        }
        [HttpGet("GenerateBill/{id}")]
        public async Task<IActionResult> GenerateBill(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound();

            var totalDays = (reservation.EndDate - reservation.StartDate).Days;
            var dailyRate = reservation.Vehicle?.RentalPricePerDay ?? 0;
            var subtotal = totalDays * dailyRate;
            var taxRate = 0.05;
            var taxAmount = subtotal * taxRate;
            var insuranceFee = 30.0;
            var cleaningFee = 25.0;
            var totalAmount = subtotal + taxAmount + insuranceFee + cleaningFee;

            var billing = new Billing
            {

                CustomerId = reservation.CustomerId,
                ReservationId = reservation.Id,
                Tax = taxRate,
                InsuranceFee = insuranceFee,
                CleaningFee = cleaningFee,
                TotalAmount = totalAmount,
                BillingDate = DateTime.Now
            };

            _context.Billings.Add(billing);
            await _context.SaveChangesAsync();

            return View("BillDetails", billing);
        }

        [HttpGet("BillDetails/{id}")]
        public async Task<IActionResult> BillDetails(int id)
        {
            var bill = await _context.Billings
                .Include(b => b.Customer)
                .Include(b => b.Reservation)
                .ThenInclude(r => r.Vehicle)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null)
                return NotFound();

            return View(bill);
        }


    }
}
