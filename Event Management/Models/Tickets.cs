using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Event_Management.Models
{
    public class Tickets
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid EventId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TicketPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BookingFee { get; set; }
        [Required]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? PaymentStatus { get; set; }

        [ForeignKey("EventId")]
        public Eventsss Event { get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; }
    }
}
