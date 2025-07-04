namespace Event_Management.ViewModels
{
    public class EventListViewModel
    {
        public List<Eventpro.Domain.Models.Events> Events { get; set; } = new();
        public int EventsCount { get; set; }
        public List<Eventpro.Domain.Models.Users> Organizers { get; set; } = new();
    }
}
