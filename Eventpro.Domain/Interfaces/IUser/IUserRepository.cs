using Eventpro.Domain.Models;
namespace Eventpro.Domain.Interfaces.IUser
{
    public interface IUserRepository
    {
        Task<Users?> GetByEmailAndRoleAsync(string email, string role);
        Task<Users?> GetByIdAsync(Guid id);
        Task<bool> ExistsByEmailAsync(string email);
        Task AddAsync(Users user);
        void Update(Users user);
        Task DeleteAsync(Users user);         
        Task<IEnumerable<Users>> GetAllAsync(); 
        Task SaveChangesAsync();
    }

}
