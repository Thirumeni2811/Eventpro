using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Management.Models
{
    public class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Organizer name is required")]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string? Person { get; set; }  

        [MaxLength(255)]
        public string? Address { get; set; }  

        [Required(ErrorMessage = "Email is mandatory")]
        [EmailAddress(ErrorMessage = "Enter a valid email")]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Enter a valid phone number")]
        [MaxLength(15)]
        public string PhoneNo { get; set; }

        [DataType(DataType.Password)]
        [MaxLength(100)]
        public string? Password { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? CPassword { get; set; }

        [Url(ErrorMessage = "Enter a valid URL")]
        [MaxLength(100)]
        public string? Website { get; set; }

        [MaxLength(20)]
        public string? Role { get; set; } 

        [NotMapped]
        [BindNever]
        public IFormFile? ImageFile { get; set; }

        public string? Image { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? CreatedAt { get; set; }  
    }

}
