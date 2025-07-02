using Eventpro.DataAccess;
using Eventpro.Domain.Exceptions;
using Eventpro.Domain.Interfaces.IUser;
using Eventpro.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Eventpro.DataAccess
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Users?> GetByEmailAndRoleAsync(string email, string role)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Role == role);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error retrieving user by email and role.", ex);
            }
        }

        public async Task<Users?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Users.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error retrieving user by ID.", ex);
            }
        }

        public async Task<IEnumerable<Users>> GetAllAsync()
        {
            try
            {
                return await _context.Users.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error retrieving all users.", ex);
            }
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Email == email);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error checking if email exists.", ex);
            }
        }

        public async Task AddAsync(Users user)
        {
            try
            {
                await _context.Users.AddAsync(user);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error adding user.", ex);
            }
        }

        public void Update(Users user)
        {
            try
            {
                _context.Users.Update(user);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error updating user.", ex);
            }
        }

        public async Task DeleteAsync(Users user)
        {
            try
            {
                _context.Users.Remove(user);
                await Task.CompletedTask; 
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error deleting user.", ex);
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("Error saving changes.", ex);
            }
        }
    }
}
