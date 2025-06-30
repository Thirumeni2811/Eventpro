namespace Event_Management.Models
{
    public class EventsView
    {
        public Users User { get; set; }
        public List<Events> Events { get; set; }
        public Events Event { get; set; }
        public List<Tickets> Tickets { get; set; }

        public string SearchQuery { get; set; }
        public string StatusFilter { get; set; }
        public string Role { get; set; }

        public int? RemainingTickets { get; set; }

        public Users Organizer { get; set; }

        public int TicketsCount { get; set; }
        public List<TicketTypeCount> TicketTypeCounts { get; set; }
        public decimal TotalTicketPrice { get; set; }
        public decimal TotalBookingFee { get; set; }
        public decimal GrandTotalPrice { get; set; }

    }

    public class TicketTypeCount
    {
        public string Type { get; set; }
        public int Quantity { get; set; }
    }
}
