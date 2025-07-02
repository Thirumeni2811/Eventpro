using System;

namespace Eventpro.Domain.Models
{
    public class Gallery
    {
        public Guid Id { get; set; }
        public string? Banner { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
