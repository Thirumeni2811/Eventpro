using Eventpro.Domain.Models;

namespace Event_Management.ViewModels
{
    public class OrganizerEventsViewModel
    {
        public Users User { get; set; }
        public List<Events> Events { get; set; } = new List<Events>();
        public string Role { get; set; }
    }
}
