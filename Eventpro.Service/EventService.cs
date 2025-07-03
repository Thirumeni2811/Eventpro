using Eventpro.Domain.Exceptions;
using Eventpro.Domain.Interfaces.IEvents;
using Eventpro.Domain.Interfaces.ITicket;
using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;

namespace Eventpro.Service
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _repository;
        private readonly IServiceResponseFactory _responseFactory;

        public EventService(
            IEventRepository repository,
            IServiceResponseFactory responseFactory)
        {
            _repository = repository;
            _responseFactory = responseFactory;
        }

        // GET ALL EVENTS
        public async Task<IServiceResponse<IEnumerable<Events>>> GetAllEventsAsync(
            string actingRole,
            Guid? eventId = null,
            string name = null,
            string organizedBy = null,
            string type = null,
            string venue = null,
            string status = null,
            string isPaid = null)
        {
            try
            {
                if (actingRole != "Admin")
                    return _responseFactory.CreateResponse<IEnumerable<Events>>(
                        false, "Unauthorized.", ActionType.Unauthorized
                    );

                var list = await _repository.GetFilteredAsync(
                    eventId, name, organizedBy, type, venue, status, isPaid
                );

                return _responseFactory.CreateResponse(
                    true, "Events retrieved.", ActionType.Retrieved, list
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving events.", ex);
            }
        }

        // GET PUBLIC EVENTS
        public async Task<IServiceResponse<IEnumerable<Events>>> GetPublicEventsAsync(
            string name = null,
            string status = null,
            string type = null,
            string venue = null)
        {
            try
            {
                var events = await _repository.GetFilteredPublicEventsAsync(
                    name, status, type, venue);

                bool isUpdated = false;
                foreach (var ev in events)
                {
                    if (ev.Status == "Upcoming" && ev.DateTime < DateTime.UtcNow)
                    {
                        ev.Status = "Completed";
                        isUpdated = true;
                    }
                }

                if (isUpdated)
                {
                    await _repository.SaveChangesAsync();
                }

                return _responseFactory.CreateResponse(
                    true,
                    "Public events retrieved.",
                    ActionType.Retrieved,
                    events);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving public events.", ex);
            }
        }

        // GET EVENT BY EVENT ID
        public async Task<IServiceResponse<Events>> GetEventByIdAsync(
            Guid id )
        {
            try
            {
                var evt = await _repository.GetByIdAsync(id);
                if (evt == null)
                    return _responseFactory.CreateResponse<Events>(
                        false, "Event not found.", ActionType.NotFound
                    );

                return _responseFactory.CreateResponse(
                    true, "Event retrieved.", ActionType.Retrieved, evt
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving event.", ex);
            }
        }

        // GET EVENT BY USER ID
        public async Task<IServiceResponse<IEnumerable<Events>>> GetEventsByUserIdAsync(
            Guid userId,
            string eventName,
            string status)
        {
            try
            {
                var events = await _repository.GetByUserIdAsync(userId);

                if (events == null)
                {
                    return _responseFactory.CreateResponse<IEnumerable<Events>>(
                        true,
                        "No events found.",
                        ActionType.Retrieved,
                        Enumerable.Empty<Events>());
                }

                // Filter
                var filtered = events.AsQueryable();

                if (!string.IsNullOrWhiteSpace(eventName))
                {
                    eventName = eventName.Trim();
                    filtered = filtered.Where(e => e.Name.Contains(eventName));
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    filtered = filtered.Where(e => e.Status == status);
                }

                var filteredList = filtered.ToList(); 

                bool isUpdated = false;
                foreach (var ev in filteredList)
                {
                    if (ev.Status == "Upcoming" && ev.DateTime < DateTime.UtcNow)
                    {
                        ev.Status = "Completed";
                        isUpdated = true;
                    }
                }

                if (isUpdated)
                {
                    await _repository.SaveChangesAsync();
                }

                return _responseFactory.CreateResponse<IEnumerable<Events>>(
                    true,
                    "Events retrieved.",
                    ActionType.Retrieved,
                    filteredList.AsEnumerable());
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving events with filter.", ex);
            }
        }


        // CREATE EVENT
        public async Task<IServiceResponse<Events>> CreateEventAsync(
            Events evt, string actingRole, Guid actingUserId)
        {
            try
            {
                if (actingRole != "Organizer")
                    return _responseFactory.CreateResponse<Events>(
                        false, "Unauthorized.", ActionType.Unauthorized
                    );

                evt.Id = Guid.NewGuid();
                evt.UserId = actingUserId;
                evt.CreatedAt = DateTime.UtcNow;

                await _repository.AddAsync(evt);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(
                    true, "Event created.", ActionType.Created, evt
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error creating event.", ex);
            }
        }

        // UPDATE EVENT
        public async Task<IServiceResponse<Events>> UpdateEventAsync(
            Events evt, string actingRole, Guid actingUserId)
        {
            try
            {
                if (actingRole != "Organizer")
                    return _responseFactory.CreateResponse<Events>(
                        false, "Unauthorized.", ActionType.Unauthorized);

                var existing = await _repository.GetByIdAsync(evt.Id);
                if (existing == null)
                    return _responseFactory.CreateResponse<Events>(
                        false, "Event not found.", ActionType.NotFound);

                if (existing.UserId != actingUserId)
                    return _responseFactory.CreateResponse<Events>(
                        false, "Cannot update another’s event.", ActionType.Forbidden);

                existing.Name = evt.Name;
                existing.Type = evt.Type;
                existing.Description = evt.Description;
                existing.DateTime = evt.DateTime;
                existing.Duration = evt.Duration;
                existing.Venue = evt.Venue;
                existing.VenueName = evt.VenueName;
                existing.Address = evt.Address;
                existing.Environment = evt.Environment;
                existing.Capacity = evt.Capacity;
                existing.Accessibility = evt.Accessibility;
                existing.IsPaid = evt.IsPaid;
                existing.TicketPricing = evt.TicketPricing;
                existing.Payment = evt.Payment;
                existing.MaxAttendees = evt.MaxAttendees;
                existing.RegistrationDeadline = evt.RegistrationDeadline;
                existing.CancellationPolicy = evt.CancellationPolicy;
                existing.Agenda = evt.Agenda;
                existing.Activities = evt.Activities;
                existing.Speakers = evt.Speakers;
                existing.Breaks = evt.Breaks;
                existing.Banner = evt.Banner;
                existing.Platforms = evt.Platforms;
                existing.Audience = evt.Audience;
                existing.Sponsors = evt.Sponsors;
                existing.SoundSystem = evt.SoundSystem;
                existing.Projection = evt.Projection;
                existing.LiveStreaming = evt.LiveStreaming;
                existing.Internet = evt.Internet;
                existing.PowerBackup = evt.PowerBackup;
                existing.Volunteers = evt.Volunteers;
                existing.Security = evt.Security;
                existing.Coordinators = evt.Coordinators;
                existing.Medical = evt.Medical;
                existing.Veg = evt.Veg;
                existing.NonVeg = evt.NonVeg;
                existing.Menu = evt.Menu;
                existing.ServingStyle = evt.ServingStyle;
                existing.GuestCount = evt.GuestCount;
                existing.Feedback = evt.Feedback;
                existing.Media = evt.Media;
                existing.Report = evt.Report;
                existing.Thanks = evt.Thanks;
                existing.Message = evt.Message;

                await _repository.UpdateAsync(existing);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(
                    true, "Event updated.", ActionType.Updated, existing);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error updating event.", ex);
            }
        }


        // DELETE EVENT
        public async Task<IServiceResponse<bool>> DeleteEventAsync(
            Guid id, string actingRole, Guid actingUserId)
        {
            try
            {
                if (actingRole != "Organizer")
                    return _responseFactory.CreateResponse<bool>(
                        false, "Unauthorized.", ActionType.Unauthorized
                    );

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    return _responseFactory.CreateResponse<bool>(
                        false, "Event not found.", ActionType.NotFound
                    );

                if (existing.UserId != actingUserId)
                    return _responseFactory.CreateResponse<bool>(
                        false, "Cannot delete another’s event.", ActionType.Forbidden
                    );

                await _repository.DeleteAsync(existing);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(
                    true, "Event deleted.", ActionType.Deleted, true
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error deleting event.", ex);
            }
        }
    }
}
