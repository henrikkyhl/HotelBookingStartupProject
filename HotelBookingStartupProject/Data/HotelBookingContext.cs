using Microsoft.EntityFrameworkCore;
using HotelBookingStartupProject.Models;

namespace HotelBookingStartupProject.Data
{
    public class HotelBookingContext : DbContext
    {
        public HotelBookingContext (DbContextOptions<HotelBookingContext> options)
            : base(options)
        {
        }

        public DbSet<Booking> Booking { get; set; }

        public DbSet<Room> Room { get; set; }

        public DbSet<Customer> Customer { get; set; }
    }
}
