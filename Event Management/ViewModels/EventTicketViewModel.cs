using Eventpro.Domain.Models;

namespace Event_Management.ViewModels
{
    public class EventTicketViewModel
    {
        public Users User { get; set; }
        public Events Event { get; set; }
        public Users Organizer { get; set; }
        public int? RemainingTickets { get; set; }
    }
}
