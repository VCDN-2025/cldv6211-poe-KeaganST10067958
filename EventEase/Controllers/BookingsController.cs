using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Models.ViewModels;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Bookings.Include(b => b.Event).Include(b => b.Venue);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name");
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,VenueId,EventId,StartDateTime,EndDateTime")] Booking booking)
        {
            if (booking.StartDateTime >= booking.EndDateTime)
            {
                ModelState.AddModelError("", "End time must be after start time.");
            }

            bool isOverlapping = await _context.Bookings.AnyAsync(b =>
                b.VenueId == booking.VenueId &&
                (booking.StartDateTime < b.EndDateTime && booking.EndDateTime > b.StartDateTime)
            );

            if (isOverlapping)
            {
                var venue = await _context.Venues.FindAsync(booking.VenueId);
                ModelState.AddModelError("", $"❌ Conflict: The venue '{venue?.Name}' is already booked during this time slot.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", booking.VenueId);
            return View(booking);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", booking.VenueId);
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,VenueId,EventId,StartDateTime,EndDateTime")] Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Name", booking.EventId);
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "Name", booking.VenueId);
            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
        public async Task<IActionResult> Summary(string searchEvent, string venueFilter, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Bookings
                .Include(b => b.Venue)
                .Include(b => b.Event)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchEvent))
            {
                query = query.Where(b => b.Event.Name.Contains(searchEvent));
            }

            if (!string.IsNullOrEmpty(venueFilter))
            {
                query = query.Where(b => b.Venue.Name == venueFilter);
            }

            if (startDate.HasValue)
            {
                query = query.Where(b => b.StartDateTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(b => b.EndDateTime <= endDate.Value);
            }

            var summary = await query
                .Select(b => new BookingSummaryViewModel
                {
                    BookingId = b.BookingId,
                    VenueName = b.Venue.Name,
                    VenueLocation = b.Venue.Location,
                    EventName = b.Event.Name,
                    EventType = b.Event.EventType,
                    StartDateTime = b.StartDateTime,
                    EndDateTime = b.EndDateTime
                })
                .ToListAsync();

            // Pass unique venue list to the view
            ViewBag.Venues = await _context.Venues
                .Select(v => v.Name)
                .Distinct()
                .ToListAsync();

            return View(summary);
        }

    }
}
