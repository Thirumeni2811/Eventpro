using Eventpro.Domain.Models;
namespace Eventpro.Domain.Interfaces.IUser
{
    public interface IUserRepository
    {
        Task<Users?> GetByEmailAndRoleAsync(string email, string role);
        Task<Users?> GetByIdAsync(Guid id);
        Task<Users?> GetByEmailAsync(string email);

        Task<bool> ExistsByEmailAsync(string email);
        Task AddAsync(Users user);
        void Update(Users user);
        Task DeleteAsync(Users user);         
        Task<IEnumerable<Users>> GetAllAsync();
        Task<IEnumerable<Users>> GetFilteredAsync(
            string? userId = null,
            string? name = null,
            string? email = null,
            string? phoneNo = null,
            string? role = null
        );
        Task<IEnumerable<Users>> GetOrganizersAsync();
        Task<IEnumerable<Users>> GetBuyersAsync();

        Task SaveChangesAsync();
    }

}
