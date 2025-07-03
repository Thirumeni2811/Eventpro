namespace Eventpro.Domain.Models
{
    public class Events
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Users User { get; set; }

        // Step 1: Basic Info
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
        public string? Theme { get; set; }
        public DateTime? DateTime { get; set; }
        public decimal? Duration { get; set; }
        public string? Venue { get; set; }

        // Step 2: Venue
        public string? VenueName { get; set; }
        public string? Address { get; set; }
        public string? Environment { get; set; }
        public int? Capacity { get; set; }
        public string? Accessibility { get; set; }

        // Step 3: Tickets
        public string? IsPaid { get; set; } = "Free";
        public string? TicketPricing { get; set; }
        public string? Payment { get; set; }
        public int? MaxAttendees { get; set; }
        public DateTime? RegistrationDeadline { get; set; }
        public string? CancellationPolicy { get; set; }

        // Step 4: Schedule
        public string? Agenda { get; set; }
        public string? Activities { get; set; }
        public string? Speakers { get; set; }
        public string? Breaks { get; set; }

        // Step 5: Promotions
        public string? Banner { get; set; }
        public string? Platforms { get; set; }
        public string? Audience { get; set; }
        public string? Sponsors { get; set; }

        // Step 6: Technical
        public bool SoundSystem { get; set; }
        public bool Projection { get; set; }
        public bool LiveStreaming { get; set; }
        public bool Internet { get; set; }
        public bool PowerBackup { get; set; }

        // Step 7: Staffs
        public bool Volunteers { get; set; }
        public bool Security { get; set; }
        public bool Coordinators { get; set; }
        public bool Medical { get; set; }

        // Step 8: Catering
        public bool Veg { get; set; }
        public bool NonVeg { get; set; }
        public string? Menu { get; set; }
        public string? ServingStyle { get; set; }
        public int? GuestCount { get; set; }

        // Step 9: Post Events
        public bool Feedback { get; set; }
        public bool Media { get; set; }
        public bool Report { get; set; }
        public bool Thanks { get; set; }

        // Final
        public string? Status { get; set; } = "Upcoming";
        public string? Message { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
