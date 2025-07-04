using Eventpro.Domain.Models;

namespace Event_Management.ViewModels
{
    public class AllEventsViewModel
    {
        public Users User { get; set; }
        public List<Events> Events { get; set; }
        public string Role { get; set; }
    }
}
