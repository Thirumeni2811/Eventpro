using Eventpro.Domain.Models;

namespace Event_Management.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Gallery> Galleries { get; set; } = new List<Gallery>();
        public List<Provides> Provides { get; set; } = new List<Provides>();
        public List<Services> Services { get; set; } = new List<Services>();
        public string? Role { get; set; }
    }
}

