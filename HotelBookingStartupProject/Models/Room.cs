using System.Collections.Generic;

namespace HotelBookingStartupProject.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public virtual List<Booking> Bookings { get; set; }
    }
}
