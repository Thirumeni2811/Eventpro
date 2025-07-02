using Eventpro.Domain.Exceptions;
using Eventpro.Domain.Interfaces.IProvide;
using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;

namespace Eventpro.Service
{
    public class ProvideService : IProvideService
    {
        private readonly IProvideRepository _repository;
        private readonly IServiceResponseFactory _responseFactory;

        public ProvideService(IProvideRepository repository, IServiceResponseFactory responseFactory)
        {
            _repository = repository;
            _responseFactory = responseFactory;
        }

        // Get All
        public async Task<IServiceResponse<IEnumerable<Provides>>> GetAllAsync(string? titleFilter = null)
        {
            try
            {
                var query = await _repository.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(titleFilter))
                    query = query.Where(s => s.Title.Contains(titleFilter.Trim(), StringComparison.OrdinalIgnoreCase));

                return _responseFactory.CreateResponse(
                    true,
                    "Provide items retrieved successfully.",
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
        public async Task<IServiceResponse<Provides?>> GetByIdAsync(Guid id)
        {
            try
            {
                var serv = await _repository.GetByIdAsync(id);
                if (serv == null)
                {
                    return _responseFactory.CreateResponse<Provides?>(
                        false,
                        "Provide not found.",
                        ActionType.NotFound
                    );
                }

                return _responseFactory.CreateResponse(
                    true,
                    "Provide retrieved successfully.",
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
        public async Task<IServiceResponse<Provides>> CreateAsync(Provides prov, string actingRole)
        {
            try
            {
                if (actingRole != "Admin")
                {
                    return _responseFactory.CreateResponse<Provides>(
                        false,
                        "Unauthorized: Only Admins can create provides.",
                        ActionType.Unauthorized
                    );
                }

                prov.Id = Guid.NewGuid();
                await _repository.AddAsync(prov);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(
                    true,
                    "Provide created successfully.",
                    ActionType.Created,
                    prov
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error creating service.", ex);
            }
        }

        // update
        public async Task<IServiceResponse<Provides>> UpdateAsync(Provides prov, string actingRole)
        {
            try
            {
                if (actingRole != "Admin")
                {
                    return _responseFactory.CreateResponse<Provides>(
                        false,
                        "Unauthorized: Only Admins can update provides.",
                        ActionType.Unauthorized
                    );
                }

                var existing = await _repository.GetByIdAsync(prov.Id);
                if (existing == null)
                {
                    return _responseFactory.CreateResponse<Provides>(
                        false,
                        "Provide not found.",
                        ActionType.NotFound
                    );
                }

                existing.Title = prov.Title;
                existing.Description = prov.Description;
                existing.Img = prov.Img;

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
                var prov = await _repository.GetByIdAsync(id);
                if (prov == null)
                {
                    return _responseFactory.CreateResponse<bool>(
                        false,
                        "Service not found.",
                        ActionType.NotFound
                    );
                }

                await _repository.DeleteAsync(prov);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(
                    true,
                    "Provide deleted successfully.",
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
