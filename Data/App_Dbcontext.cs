using Microsoft.EntityFrameworkCore;
using Vehicle_Rental_Management_System.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace Vehicle_Rental_Management_System.Data
{
    public class App_Dbcontext : IdentityDbContext<AppUser>
    {
        public App_Dbcontext(DbContextOptions<App_Dbcontext> options) : base(options)
        {
        }
    }
}
