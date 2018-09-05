using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HotelBookingStartupProject.Models;
using HotelBookingStartupProject.Data;

namespace HotelBookingStartupProject.Controllers
{
    public class BookingsController : Controller
    {
        private readonly HotelBookingContext _context;

        public BookingsController(HotelBookingContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index(int? id)
        {
            var bookings = _context.Booking.Include(b => b.Customer).Include(b => b.Room);

            var bookingStartDates = bookings.Select(b => b.StartDate);
            DateTime minBookingDate = bookingStartDates.Any() ? bookingStartDates.Min() : DateTime.MinValue;

            var bookingEndDates = bookings.Select(b => b.EndDate);
            DateTime maxBookingDate = bookingEndDates.Any() ? bookingEndDates.Max() : DateTime.MaxValue;


            List<DateTime> fullyOccupiedDates = new List<DateTime>();

            int noOfRooms = _context.Room.Count();

            if (_context.Booking.Any())
            {
                for (DateTime d = minBookingDate; d <= maxBookingDate; d = d.AddDays(1))
                {
                    var noOfBookings = from b in _context.Booking
                                       where b.IsActive && d >= b.StartDate && d <= b.EndDate
                                       select b;
                    if (noOfBookings.Count() >= noOfRooms)
                        fullyOccupiedDates.Add(d);
                }
            }

            ViewBag.FullyOccupiedDates = fullyOccupiedDates;

            int minBookingYear = minBookingDate.Year;
            int maxBookingYear = maxBookingDate.Year;
            if (id == null)
                id = DateTime.Today.Year;
            else if (id < minBookingYear)
                id = minBookingYear;
            else if (id > maxBookingYear)
                id = maxBookingYear;

            ViewBag.YearToDisplay = id;

            return View(await bookings.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Customer)
                .Include(b => b.Room)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Set<Customer>(), "Id", "Id");
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StartDate,EndDate,CustomerId")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                int roomId = -1;
                DateTime startDate = booking.StartDate;
                DateTime endDate = booking.EndDate;

                if (startDate <= DateTime.Today || startDate > endDate)
                {
                    ViewData["CustomerId"] = new SelectList(_context.Set<Customer>(), "Id", "Id", booking.CustomerId);
                    ViewBag.Status = "The start date cannot be in the past or later than the end date.";
                    return View(booking);
                }

                var activeBookings = _context.Booking.Where(b => b.IsActive);
                foreach (var room in _context.Room)
                {
                    var activeBookingsForCurrentRoom = activeBookings.Where(b => b.RoomId == room.Id);
                    if (activeBookingsForCurrentRoom.All(b => startDate < b.StartDate &&
                        endDate < b.StartDate || startDate > b.EndDate && endDate > b.EndDate))
                    {
                        roomId = room.Id;
                        break;
                    }
                }

                if (roomId >= 0)
                {
                    booking.RoomId = roomId;
                    booking.IsActive = true;
                    _context.Booking.Add(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["CustomerId"] = new SelectList(_context.Set<Customer>(), "Id", "Id", booking.CustomerId);
            ViewBag.Status = "The booking could not be created. There were no available room.";
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking.SingleOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Set<Customer>(), "Id", "Id", booking.CustomerId);
            ViewData["RoomId"] = new SelectList(_context.Set<Room>(), "Id", "Id", booking.RoomId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StartDate,EndDate,IsActive,CustomerId,RoomId")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Set<Customer>(), "Id", "Id", booking.CustomerId);
            ViewData["RoomId"] = new SelectList(_context.Set<Room>(), "Id", "Id", booking.RoomId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Customer)
                .Include(b => b.Room)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.SingleOrDefaultAsync(m => m.Id == id);
            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.Id == id);
        }
    }
}
