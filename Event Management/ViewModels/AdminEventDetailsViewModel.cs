using Eventpro.Domain.Models;

namespace Event_Management.ViewModels
{
    public class AdminEventDetailsViewModel
    {
        public Events Event { get; set; }
        public List<Tickets> Tickets { get; set; }
        public Users Organizer { get; set; }
        public List<TicketTypeCountViewModel> TicketTypeCounts { get; set; }
        public int TotalTickets { get; set; }
        public int? RemainingTickets { get; set; }
        public decimal TotalTicketPrice { get; set; }
        public decimal TotalBookingFee { get; set; }
        public decimal GrandTotalPrice { get; set; }
    }

    public class TicketTypeCountViewModel
    {
        public string Type { get; set; }
        public int Quantity { get; set; }
    }
}
