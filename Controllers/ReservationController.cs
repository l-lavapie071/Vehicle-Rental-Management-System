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
        public async Task<IActionResult> ReservationList()
        {
            var reservations = await _context.Reservations
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .ToListAsync();
            return View(reservations);
        }

        // Get Customer Details
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> ReservationDetails(int id)
        {
            var reservation = await _context.Reservations
            .Where(r => r.Id == id)
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync();

            return reservation != null ? View(reservation) : NotFound();
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

                // Check if vehicle is changed
                if (oldVehicle.Id != newVehicle.Id)
                {
                    oldVehicle.IsAvailable = true;


                    // Set the new vehicle as not available
                    newVehicle.IsAvailable = false;


                    if (viewModel.Reservation.IsReturned)
                    {
                        newVehicle.IsAvailable = true;
                    }
                    else {
                        newVehicle.IsAvailable = false;
                    }
                    // Update the vehicles in the database
                    _context.Update(oldVehicle);
                    _context.Update(newVehicle);
                }
                else {
                    if (viewModel.Reservation.IsReturned)
                    {
                        oldVehicle.IsAvailable = true;
                        
                    }
                    else {
                        oldVehicle.IsAvailable = false;
                    }
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




    }
}
