using System.Collections.Generic;

namespace Event_Management.Models
{
    public class HomeView
    {
        public List<Gallery> Gallery { get; set; }
        public List<Provide> Provides { get; set; }
        public List<Service> Services { get; set; }
        public string? Role { get; set; }
    }
}
