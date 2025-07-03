using Eventpro.Domain.Exceptions;
using Eventpro.Domain.Interfaces.ITicket;
using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;

namespace Eventpro.Service
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _repository;
        private readonly IServiceResponseFactory _responseFactory;

        public TicketService(
            ITicketRepository repository,
            IServiceResponseFactory responseFactory)
        {
            _repository = repository;
            _responseFactory = responseFactory;
        }

        // get all
        public async Task<IServiceResponse<IEnumerable<Tickets>>> GetAllAsync(
            string actingRole,
            Guid? ticketId = null,
            Guid? eventId = null,
            string eventName = null,
            string organizerName = null,
            string buyerName = null)
        {
            try
            {
                if (actingRole != "Admin")
                {
                    return _responseFactory.CreateResponse<IEnumerable<Tickets>>(
                        false,
                        "Unauthorized.",
                        ActionType.Unauthorized
                    );
                }

                var tickets = await _repository.GetFilteredAsync(
                    ticketId,
                    eventId,
                    eventName,
                    organizerName,
                    buyerName
                );

                return _responseFactory.CreateResponse(
                    true,
                    "Tickets retrieved successfully.",
                    ActionType.Retrieved,
                    tickets
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving tickets.", ex);
            }
        }

        // get tickets by user id
        public async Task<IServiceResponse<IEnumerable<Tickets>>> GetTicketsByUserIdAsync(Guid userId, string actingRole)
        {
            try
            {
                if (actingRole != "User")
                    return _responseFactory.CreateResponse<IEnumerable<Tickets>>(
                        false, "Unauthorized.", ActionType.Unauthorized);

                var tickets = await _repository.GetByUserIdAsync(userId);

                return _responseFactory.CreateResponse(
                    true, "Tickets retrieved.", ActionType.Retrieved, tickets);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving tickets by user.", ex);
            }
        }

        // get ticket count by user id
        public async Task<IServiceResponse<int>> GetTicketCountByUserIdAsync(Guid userId, string actingRole)
        {
            try
            {
                if (actingRole != "User")
                    return _responseFactory.CreateResponse<int>(
                        false, "Unauthorized.", ActionType.Unauthorized);

                var count = await _repository.GetCountByUserIdAsync(userId);

                return _responseFactory.CreateResponse(
                    true, "Ticket count retrieved.", ActionType.Retrieved, count);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving ticket count.", ex);
            }
        }

        // get tickets by event id
        public async Task<IServiceResponse<IEnumerable<Tickets>>> GetTicketsByEventIdAsync(Guid eventId, string actingRole)
        {
            try
            {
                if (actingRole == "User")
                    return _responseFactory.CreateResponse<IEnumerable<Tickets>>(
                        false, "Unauthorized.", ActionType.Unauthorized);

                var tickets = await _repository.GetByEventIdAsync(eventId);

                return _responseFactory.CreateResponse(
                    true, "Tickets retrieved.", ActionType.Retrieved, tickets);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving tickets by event.", ex);
            }
        }

        // get ticket type count by event id
        public async Task<IServiceResponse<IEnumerable<(string Type, int Quantity)>>> GetTicketTypeCountsByEventIdAsync(Guid eventId)
        {
            try
            {
                var counts = await _repository.GetTicketTypeCountsByEventIdAsync(eventId);

                return _responseFactory.CreateResponse(
                    true, "Ticket types retrieved.", ActionType.Retrieved, counts);
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving ticket type counts.", ex);
            }
        }
    }
}
