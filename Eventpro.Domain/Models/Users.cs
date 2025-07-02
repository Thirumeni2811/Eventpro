
namespace Eventpro.Domain.Models
{
    public class Users
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Person { get; set; }
        public string? Address { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string? Password { get; set; }
        public string? CPassword { get; set; }
        public string? Website { get; set; }
        public string? Role { get; set; }
        public string? Image { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
