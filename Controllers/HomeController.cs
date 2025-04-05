using Microsoft.AspNetCore.Mvc;

namespace Vehicle_Rental_Management_System.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("home")]
        public IActionResult Index()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("", "App");
            }
            return View();
        }
    }
}
