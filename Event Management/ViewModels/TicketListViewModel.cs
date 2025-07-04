namespace Event_Management.ViewModels
{
    public class TicketListViewModel
    {
        // The list of tickets to display
        public List<Eventpro.Domain.Models.Tickets> Tickets { get; set; } = new();

        // Dropdown data (optional)
        public List<Eventpro.Domain.Models.Users> Organizers { get; set; } = new();
        public List<Eventpro.Domain.Models.Users> Buyers { get; set; } = new();
        public List<Eventpro.Domain.Models.Events> Events { get; set; } = new();
    }
}
