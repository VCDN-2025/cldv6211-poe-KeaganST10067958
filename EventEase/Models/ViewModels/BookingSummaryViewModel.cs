namespace EventEase.Models.ViewModels
{
    public class BookingSummaryViewModel
    {
        public int BookingId { get; set; }

        public string VenueName { get; set; }
        public string VenueLocation { get; set; }

        public string EventName { get; set; }
        public string EventType { get; set; }

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
