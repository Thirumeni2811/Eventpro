using Eventpro.Domain.Models;

namespace Event_Management.ViewModels
{
    public class OrganizerEventDetailViewModel
    {
        public Users User { get; set; }
        public Events Event { get; set; }
        public string Role { get; set; }
        public int TicketsCount { get; set; }
        public List<Tickets> Tickets { get; set; } = new List<Tickets>();
        public List<TicketTypeCount> TicketTypeCounts { get; set; } = new List<TicketTypeCount>();
    }

    public class TicketTypeCount
    {
        public string Type { get; set; }
        public int Quantity { get; set; }
    }
}
