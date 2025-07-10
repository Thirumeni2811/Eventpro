using Eventpro.Domain.Exceptions;
using Eventpro.Domain.Interfaces.IServ;
using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;

namespace Eventpro.Service
{
    public class ServService : IServService
    {
        private readonly IServRepository _repository;
        private readonly IServiceResponseFactory _responseFactory;

        public ServService(IServRepository repository, IServiceResponseFactory responseFactory)
        {
            _repository = repository;
            _responseFactory = responseFactory;
        }

        // Get All
        public async Task<IServiceResponse<IEnumerable<Services>>> GetAllAsync(string? titleFilter = null)
        {
            try
            {
                var query = await _repository.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(titleFilter))
                    query = query.Where(s => s.Title.Contains(titleFilter.Trim(), StringComparison.OrdinalIgnoreCase));

                return _responseFactory.CreateResponse(
                    true,
                    "Service items retrieved successfully.",
                    ActionType.Retrieved,
                    query
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving services.", ex);
            }
        }

        // Get by Id
        public async Task<IServiceResponse<Services?>> GetByIdAsync(Guid id)
        {
            try
            {
                var serv = await _repository.GetByIdAsync(id);
                if (serv == null)
                {
                    return _responseFactory.CreateResponse<Services?>(
                        false,
                        "Service not found.",
                        ActionType.NotFound
                    );
                }

                return _responseFactory.CreateResponse(
                    true,
                    "Service retrieved successfully.",
                    ActionType.Retrieved,
                    serv
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving service.", ex);
            }
        }

        // create
        public async Task<IServiceResponse<Services>> CreateAsync(Services serv)
        {
            try
            {
                serv.Id = Guid.NewGuid();
                await _repository.AddAsync(serv);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(
                    true,
                    "Service created successfully.",
                    ActionType.Created,
                    serv
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error creating service.", ex);
            }
        }

        // update
        public async Task<IServiceResponse<Services>> UpdateAsync(Services serv)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(serv.Id);
                if (existing == null)
                {
                    return _responseFactory.CreateResponse<Services>(
                        false,
                        "Service not found.",
                        ActionType.NotFound
                    );
                }

                existing.Title = serv.Title;
                existing.Description = serv.Description;
                if (!string.IsNullOrEmpty(serv.Img))
                {
                    existing.Img = serv.Img;
                }

                await _repository.UpdateAsync(existing);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(
                    true,
                    "Service updated successfully.",
                    ActionType.Updated,
                    existing
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error updating service.", ex);
            }
        }

        //delete
        public async Task<IServiceResponse<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var serv = await _repository.GetByIdAsync(id);
                if (serv == null)
                {
                    return _responseFactory.CreateResponse<bool>(
                        false,
                        "Service not found.",
                        ActionType.NotFound
                    );
                }

                await _repository.DeleteAsync(serv);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(
                    true,
                    "Service deleted successfully.",
                    ActionType.Deleted,
                    true
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error deleting service.", ex);
            }
        }
    }
}
