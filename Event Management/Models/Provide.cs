using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Event_Management.Models
{
    public class Provide
    {
        public Guid Id { get; set; }
        [NotMapped]
        public IFormFile? BannerFile { get; set; }
        public string? Img { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
