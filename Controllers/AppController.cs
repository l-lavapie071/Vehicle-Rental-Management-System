using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Vehicle_Rental_Management_System.Models.ViewModels;
using Vehicle_Rental_Management_System.Models;


namespace Vehicle_Rental_Management_System.Controllers
{
    public class AppController : Controller
    {
        private readonly SignInManager<AppUser> signInManager;
        private readonly UserManager<AppUser> userManager;

        public AppController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }
        // View login page
        [HttpGet]
        [Route("/")]
        public IActionResult Login()
        {
            return View();
        }

        // Login - Authenticate user
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)  // Check if the model is valid
            {
                var result = await signInManager.PasswordSignInAsync(
                model.Username!,  // Get username from the model
                model.Password!,  // Get password from the model
                false,  // Do not remember the user
                false);  // Do not lock out on failure
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");  // Redirect to dashboard after login
                }
                ModelState.AddModelError("", "Invalid login attempt"); // Set error message for invalid login
            }


            return View(model);
        }

        // View register page
        [HttpGet]
        [Route("register")]
        public IActionResult Register()
        {
            return View();
        }

        // Register user
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    UserName = model.Username,
                    PasswordHash = model.Password,
                    Address = model.Address,

                };
                var result = await userManager.CreateAsync(user, model.Password!);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

            }

            return View(model);
        }
        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();  // Sign out the user
            return RedirectToAction("", "App");  // Redirect to login page
        }
    }
}
