using System;

namespace Eventpro.Domain.Models
{
    public class Services
    {
        public Guid Id { get; set; }
        public string? Img { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
