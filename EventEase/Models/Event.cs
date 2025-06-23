using System.ComponentModel.DataAnnotations;

namespace EventEase.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public ICollection<Booking>? Bookings { get; set; }

        public int? EventTypeId { get; set; }
        public EventType? EventType { get; set; }

    }
}
