using Vehicle_Rental_Management_System.Data;
using Vehicle_Rental_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using System.Reflection.PortableExecutable;

namespace Vehicle_Rental_Management_System.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class CustomerController : Controller
    {

        private readonly App_Dbcontext _context;

        public CustomerController(App_Dbcontext context)
        {
            _context = context;
        }

        // Display all Customer
        [HttpGet]
        public async Task<IActionResult> CustomerList(string name, string email, string phone)
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => (c.FirstName + " " + c.LastName).Contains(name));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(c => c.Email.Contains(email));
            }

            if (!string.IsNullOrWhiteSpace(phone))
            {
                query = query.Where(c => c.PhoneNumber.Contains(phone));
            }

            var filteredCustomers = await query.ToListAsync();
            return View(filteredCustomers);
        }

        // Get Customer Details
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> CustomerDetails(int id)
        {
            var cust = await _context.Customers.FindAsync(id);
            return cust != null ? View(cust) : NotFound();
        }

        // Show create form
        [HttpGet("AddCustomer")]
        public IActionResult AddCustomer()
        {
            return View();
        }

        // Add a new Reader
        [HttpPost("AddCustomer")]
        public async Task<IActionResult> AddCustomer(Customer newCustomer)
        {
            if (ModelState.IsValid)
            {
                await _context.Customers.AddAsync(newCustomer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(CustomerList));
            }
            return View(newCustomer);
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> EditCustomer(int id)
        {
            var cust = await _context.Customers.FindAsync(id);
            if (cust == null)
            {
                return NotFound();
            }
            return View(cust);
        }

        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id)
            {
                return NotFound();
            }

            // Query the existing customer from the database
            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null)
            {
                return NotFound();
            }

            // Update properties from the form, except the ImageUrl
            if (ModelState.IsValid)
            {
                try
                {

                    // Update other fields from the form to the existing vehicle object
                    existingCustomer.FirstName = customer.FirstName;
                    existingCustomer.MiddleName = customer.MiddleName;
                    existingCustomer.LastName = customer.LastName;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.PhoneNumber = customer.PhoneNumber;
                    existingCustomer.Address = customer.Address;
                   

                    // Update the vehicle record in the database
                    _context.Update(existingCustomer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Customers.Any(v => v.Id == customer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(CustomerDetails), new { id = existingCustomer.Id });
            }
            return View(customer);
        }
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var cust = await _context.Customers.FindAsync(id);
            if (cust == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(cust);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(CustomerList));
        }
    }
}
