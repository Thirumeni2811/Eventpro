using Eventpro.Domain.Models;

namespace Event_Management.ViewModels
{
    public class UserTicketsViewModel
    {
        public Users User { get; set; }
        public List<Tickets> Tickets { get; set; } = new List<Tickets>();
        public List<Events> Events { get; set; } = new List<Events>();
        public string Role { get; set; }
    }
}
