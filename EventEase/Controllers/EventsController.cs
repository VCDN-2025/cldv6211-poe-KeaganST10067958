using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobStorageService _blobService;

        public EventsController(ApplicationDbContext context, BlobStorageService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            var events = _context.Events.Include(e => e.EventType);
            return View(await events.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "EventTypeId", "Name");
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    @event.ImageUrl = await _blobService.UploadFileAsync(imageFile);
                }

                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "EventTypeId", "Name", @event.EventTypeId);
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "EventTypeId", "Name", @event.EventTypeId);
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event, IFormFile imageFile)
        {
            if (id != @event.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        @event.ImageUrl = await _blobService.UploadFileAsync(imageFile);
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["EventTypeId"] = new SelectList(_context.EventTypes, "EventTypeId", "Name", @event.EventTypeId);
            return View(@event);
        }

        // ✅ GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null) return NotFound();

            if (@event.Bookings != null && @event.Bookings.Any())
            {
                TempData["EventDeleteError"] = $"❌ Cannot delete event '{@event.Name}' because it has existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}
