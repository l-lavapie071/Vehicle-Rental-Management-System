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
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Customer> Customers { get; set; }

        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Billing> Billings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //no action on billing if reservation was deleted
            modelBuilder.Entity<Billing>()
                .HasOne(b => b.Reservation)
                .WithMany()
                .HasForeignKey(b => b.ReservationId)
                .OnDelete(DeleteBehavior.NoAction); // Specify NO ACTION for delete
            //no action on billing if reservation was deleted
            modelBuilder.Entity<Billing>()
                .HasOne(b => b.Customer)
                .WithMany()
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.NoAction); // Specify NO ACTION for delete

            base.OnModelCreating(modelBuilder);
        }
    }
}
