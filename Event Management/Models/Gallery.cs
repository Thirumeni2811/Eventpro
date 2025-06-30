using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Event_Management.Models
{
    public class Gallery
    {
        public Guid Id { get; set; }
        [NotMapped]
        public IFormFile? BannerFile { get; set; }
        public string? Banner { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
