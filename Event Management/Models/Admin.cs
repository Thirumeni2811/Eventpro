namespace Event_Management.Models
{
    public class Admin
    {
        public List<Gallery> Gallery { get; set; }
        public List<Provide> Provides { get; set; }
        public List<Service> Services { get; set; }
        public Service Service { get; set; }

        public Users User { get; set; }
        public List<Eventsss> Events { get; set; }
        public Eventsss Event { get; set; }
        public List<Tickets> Tickets { get; set; }
    }
}
