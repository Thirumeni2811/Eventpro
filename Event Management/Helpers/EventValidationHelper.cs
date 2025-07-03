using Eventpro.Domain.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Event_Management.Helpers
{
    public static class EventValidationHelper
    {
        // Step 1: Basic
        public static void ValidateBasics(Events model, ModelStateDictionary modelState)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                modelState.AddModelError("Name", "Name is required");

            if (string.IsNullOrWhiteSpace(model.Type))
                modelState.AddModelError("Type", "Type is required");

            if (string.IsNullOrWhiteSpace(model.Description))
                modelState.AddModelError("Description", "Description is required");

            if (string.IsNullOrWhiteSpace(model.Theme))
                modelState.AddModelError("Theme", "Theme is required");

            if (model.DateTime == default)
                modelState.AddModelError("DateTime", "Date and Time is required");

            if (!model.Duration.HasValue || model.Duration <= 0)
                modelState.AddModelError("Duration", "Duration is required and must be greater than 0");

            if (string.IsNullOrWhiteSpace(model.Venue))
                modelState.AddModelError("Venue", "Venue is required");
        }

        // Step 2: Venue
        public static void ValidateVenue(Events model, ModelStateDictionary modelState)
        {
            // Only validate venue details if venue is 'offline'
            if (model.Venue?.Trim().ToLower() == "offline")
            {
                if (string.IsNullOrWhiteSpace(model.VenueName))
                    modelState.AddModelError("VenueName", "Venue Name is required.");

                if (string.IsNullOrWhiteSpace(model.Address))
                    modelState.AddModelError("Address", "Address is required.");

                if (string.IsNullOrWhiteSpace(model.Environment))
                    modelState.AddModelError("Environment", "Environment is required.");

                if (!model.Capacity.HasValue || model.Capacity <= 0)
                    modelState.AddModelError("Capacity", "Capacity must be greater than 0.");

                if (string.IsNullOrWhiteSpace(model.Accessibility))
                    modelState.AddModelError("Accessibility", "Accessibility info is required.");
            }
        }

        // Step 3: Tickets
        public static void ValidateTickets(Events model, ModelStateDictionary modelState)
        {
            // Only validate ticketing if type is not 'organizer'
            if (model.Type?.Trim().ToLower() != "organizer")
            {
                if (string.IsNullOrWhiteSpace(model.IsPaid))
                    modelState.AddModelError("IsPaid", "Payment type (Paid/Free) is required.");

                if (model.IsPaid?.Trim().ToLower() == "paid")
                {
                    if (string.IsNullOrWhiteSpace(model.TicketPricing))
                        modelState.AddModelError("TicketPricing", "Ticket Pricing is required for paid events.");

                    if (string.IsNullOrWhiteSpace(model.Payment))
                        modelState.AddModelError("Payment", "Payment method is required for paid events.");

                    if (string.IsNullOrWhiteSpace(model.CancellationPolicy))
                        modelState.AddModelError("CancellationPolicy", "Cancellation Policy is required for paid events.");
                }

                if (!model.MaxAttendees.HasValue || model.MaxAttendees <= 0)
                    modelState.AddModelError("MaxAttendees", "Max Attendees must be greater than 0.");

                if (!model.RegistrationDeadline.HasValue || model.RegistrationDeadline >= model.DateTime)
                    modelState.AddModelError("RegistrationDeadline", "Registration deadline must be a future date.");
            }
        }


        // Step 4: Program
        public static void ValidateProgram(Events model, ModelStateDictionary modelState)
        {
            // Only validate program if type is not 'organizer'
            if (model.Type?.Trim().ToLower() != "organizer")
            {
                if (string.IsNullOrWhiteSpace(model.Agenda))
                    modelState.AddModelError("Agenda", "Agenda is required.");

                if (string.IsNullOrWhiteSpace(model.Activities))
                    modelState.AddModelError("Activities", "Activities are required.");

                if (string.IsNullOrWhiteSpace(model.Speakers))
                    modelState.AddModelError("Speakers", "Speakers are required.");

                if (string.IsNullOrWhiteSpace(model.Breaks))
                    modelState.AddModelError("Breaks", "Breaks info is required.");
            }
        }

        // Step 5: Promotions
        public static void ValidatePromotions(Events model, IFormFile bannerFile, ModelStateDictionary modelState)
        {
            if (string.IsNullOrEmpty(model.Banner) && (bannerFile == null || bannerFile.Length == 0))
            {
                modelState.AddModelError("BannerFile", "Banner is required.");
            }
        }

        // Step 8: Catering
        public static void ValidateCatering(Events model, ModelStateDictionary modelState)
        {
            bool hasFoodOptions = model.Veg || model.NonVeg;

            if (hasFoodOptions)
            {
                if (string.IsNullOrWhiteSpace(model.Menu))
                    modelState.AddModelError("Menu", "Menu is required when food options are selected.");

                if (string.IsNullOrWhiteSpace(model.ServingStyle))
                    modelState.AddModelError("ServingStyle", "Serving Style is required when food options are selected.");

                if (!model.GuestCount.HasValue || model.GuestCount <= 0)
                    modelState.AddModelError("GuestCount", "Guest Count must be provided and greater than 0.");
            }
        }
    }
}
