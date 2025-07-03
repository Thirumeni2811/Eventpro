using Microsoft.Extensions.Logging;
using System;

namespace Eventpro.Domain.Models
{
    public class Tickets
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TicketPrice { get; set; }
        public decimal BookingFee { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public string? PaymentStatus { get; set; }
        public Events Event { get; set; }
        public Users User { get; set; }
    }
}
