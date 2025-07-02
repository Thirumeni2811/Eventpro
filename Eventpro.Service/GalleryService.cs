using Eventpro.Domain.Interfaces.IGallery;
using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;
using Eventpro.Domain.Exceptions;

namespace Eventpro.Service
{
    public class GalleryService : IGalleryService
    {
        private readonly IGalleryRepository _repository;
        private readonly IServiceResponseFactory _responseFactory;

        public GalleryService(IGalleryRepository repository, IServiceResponseFactory responseFactory)
        {
            _repository = repository;
            _responseFactory = responseFactory;
        }

        // Get All
        public async Task<IServiceResponse<IEnumerable<Gallery>>> GetAllAsync(string? nameFilter = null, string? typeFilter = null)
        {
            try
            {
                var query = await _repository.GetAllAsync();

                if (!string.IsNullOrWhiteSpace(nameFilter))
                    query = query.Where(g => g.Name.Contains(nameFilter.Trim(), StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(typeFilter))
                    query = query.Where(g => g.Type.Equals(typeFilter.Trim(), StringComparison.OrdinalIgnoreCase));

                return _responseFactory.CreateResponse(
                    true,
                    "Gallery items retrieved successfully.",
                    ActionType.Retrieved,
                    query
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving galleries.", ex);
            }
        }

        // Get By Id
        public async Task<IServiceResponse<Gallery?>> GetByIdAsync(Guid id)
        {
            try
            {
                var gallery = await _repository.GetByIdAsync(id);
                if (gallery == null)
                {
                    return _responseFactory.CreateResponse<Gallery?>(
                        false,
                        "Gallery not found.",
                        ActionType.NotFound
                    );
                }

                return _responseFactory.CreateResponse(
                    true,
                    "Gallery retrieved successfully.",
                    ActionType.Retrieved,
                    gallery
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error retrieving gallery.", ex);
            }
        }

        // Create
        public async Task<IServiceResponse<Gallery>> CreateAsync(Gallery gallery, string actingRole)
        {
            try
            {
                if (actingRole != "Admin")
                {
                    return _responseFactory.CreateResponse<Gallery>(
                        false,
                        "Unauthorized: Only Admins can create gallery items.",
                        ActionType.Unauthorized
                    );
                }

                gallery.Id = Guid.NewGuid();
                await _repository.AddAsync(gallery);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(
                    true,
                    "Gallery created successfully.",
                    ActionType.Created,
                    gallery
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error creating gallery.", ex);
            }
        }

        // Update
        public async Task<IServiceResponse<Gallery>> UpdateAsync(Gallery gallery, string actingRole)
        {
            try
            {
                if (actingRole != "Admin")
                {
                    return _responseFactory.CreateResponse<Gallery>(
                        false,
                        "Unauthorized: Only Admins can update gallery items.",
                        ActionType.Unauthorized
                    );
                }

                var existing = await _repository.GetByIdAsync(gallery.Id);
                if (existing == null)
                {
                    return _responseFactory.CreateResponse<Gallery>(
                        false,
                        "Gallery not found.",
                        ActionType.NotFound
                    );
                }

                existing.Name = gallery.Name;
                existing.Description = gallery.Description;
                existing.Type = gallery.Type;
                existing.Banner = gallery.Banner;

                await _repository.UpdateAsync(existing);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(
                    true,
                    "Gallery updated successfully.",
                    ActionType.Updated,
                    existing
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error updating gallery.", ex);
            }
        }

        // Delete
        public async Task<IServiceResponse<bool>> DeleteAsync(Guid id)
        {
            try
            {
                var gallery = await _repository.GetByIdAsync(id);
                if (gallery == null)
                {
                    return _responseFactory.CreateResponse<bool>(
                        false,
                        "Gallery not found.",
                        ActionType.NotFound
                    );
                }

                await _repository.DeleteAsync(gallery);
                await _repository.SaveChangesAsync();

                return _responseFactory.CreateResponse(
                    true,
                    "Gallery deleted successfully.",
                    ActionType.Deleted,
                    true
                );
            }
            catch (Exception ex)
            {
                throw new ServiceException("Error deleting gallery.", ex);
            }
        }
    }
}
