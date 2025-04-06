using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vehicle_Rental_Management_System.Models;
using Vehicle_Rental_Management_System.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();



// Configure DbContext with SQL Server
builder.Services.AddDbContext<App_Dbcontext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 4;
})
.AddEntityFrameworkStores<App_Dbcontext>()
.AddDefaultTokenProviders();

// Configure Authentication with Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "UserLoginCookie"; // Set the cookie name
    options.Cookie.HttpOnly = true; // Make the cookie HTTP-only
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Use secure cookies
    options.Cookie.SameSite = SameSiteMode.Strict; // Set SameSite policy
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Set cookie expiration time
    options.SlidingExpiration = true; // Enable sliding expiration
});

var app = builder.Build();
app.UseStaticFiles();  
// Default route to the Login action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=App}/{action=Login}");

app.Run();
