using Eventpro.Domain.Exceptions;
using Eventpro.Domain.Interfaces.IUser;
using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;
using Eventpro.Service.Helpers;

namespace Eventpro.Service;
public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IServiceResponseFactory _responseFactory;
    private readonly JwtHelper _jwtHelper;

    public UserService(IUserRepository repository, IServiceResponseFactory responseFactory, JwtHelper jwtHelper)
    {
        _repository = repository;
        _responseFactory = responseFactory;
        _jwtHelper = jwtHelper;
    }

    // 1. Create User (role: User)
    public async Task<IServiceResponse<(Users User, string Token)>> CreateUserProfileAsync(Users user)
    {
        try
        {
            if (await _repository.ExistsByEmailAsync(user.Email))
            {
                return _responseFactory.CreateResponse<(Users User, string Token)>(
                    false,
                    "Email already exists.",
                    ActionType.Conflict
                );
            }

            user.Password = PasswordHelper.HashPassword(user.Password);
            user.Role = "User";
            user.CreatedAt = DateTime.UtcNow;

            await _repository.AddAsync(user);
            await _repository.SaveChangesAsync();

            string token = _jwtHelper.GenerateToken(user.Id, user.Email);

            return _responseFactory.CreateResponse(
                true,
                "User profile created successfully.",
                ActionType.Created,
                (user, token)
            );
        }
        catch (Exception ex)
        {
            throw new ServiceException("Error creating user profile.", ex);
        }
    }

    // 2. Create Organizer (role: Organizer)
    public async Task<IServiceResponse<(Users User, string Token)>> CreateOrganizerProfileAsync(Users user)
    {
        try
        {
            if (await _repository.ExistsByEmailAsync(user.Email))
            {
                return _responseFactory.CreateResponse<(Users User, string Token)>(
                    false,
                    "Email already exists.",
                    ActionType.Conflict
                );
            }

            user.Password = PasswordHelper.HashPassword(user.Password);
            user.Role = "Organizer";
            user.CreatedAt = DateTime.UtcNow;

            await _repository.AddAsync(user);
            await _repository.SaveChangesAsync();

            string token = _jwtHelper.GenerateToken(user.Id, user.Email);

            return _responseFactory.CreateResponse(
                true,
                "Organizer profile created successfully.",
                ActionType.Created,
                (user, token)
            );
        }
        catch (Exception ex)
        {
            throw new ServiceException("Error creating organizer profile.", ex);
        }
    }

    // 3. Login (User and Organizer)
    public async Task<IServiceResponse<(Users User, string Token)>> LoginOrganizerAsync(string email, string password)
    {
        return await LoginInternalAsync(email, password, "Organizer");
    }

    public async Task<IServiceResponse<(Users User, string Token)>> LoginUserAsync(string email, string password)
    {
        return await LoginInternalAsync(email, password, "User");
    }

    /// <summary>
    /// Shared logic to login with role checking
    /// </summary>
    private async Task<IServiceResponse<(Users User, string Token)>> LoginInternalAsync(string email, string password, string expectedRole)
    {
        var user = await _repository.GetByEmailAndRoleAsync(email, expectedRole);

        if (user == null)
        {
            return _responseFactory.CreateResponse<(Users User, string Token)>(
                false,
                $"{expectedRole} account not found.",
                ActionType.NotFound
            );
        }

        if (user.Role != expectedRole)
        {
            return _responseFactory.CreateResponse<(Users User, string Token)>(
                false,
                $"Access denied: Not a {expectedRole} account.",
                ActionType.Unauthorized
            );
        }

        if (!PasswordHelper.VerifyPassword(password, user.Password))
        {
            return _responseFactory.CreateResponse<(Users User, string Token)>(
                false,
                "Incorrect password.",
                ActionType.Unauthorized
            );
        }

        var token = _jwtHelper.GenerateToken(user.Id, user.Email);

        return _responseFactory.CreateResponse(
            true,
            "Login successful.",
            ActionType.Retrieved,
            (user, token)
        );
    }

    // 4. Update Profile (only Admin or the same user)
    public async Task<IServiceResponse<Users>> UpdateProfileAsync(Guid userId, Users updatedUser, string actingRole, Guid actingUserId)
    {
        try
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
            {
                return _responseFactory.CreateResponse<Users>(
                    false,
                    "User not found.",
                    ActionType.NotFound
                );
            }

            if (actingRole != "Admin" && user.Id != actingUserId)
            {
                return _responseFactory.CreateResponse<Users>(
                    false,
                    "Unauthorized to update this profile.",
                    ActionType.Unauthorized
                );
            }

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.PhoneNo = updatedUser.PhoneNo;
            user.Image = updatedUser.Image ?? user.Image;
            user.Address = updatedUser.Address;
            user.Person = updatedUser.Person;
            user.Website = updatedUser.Website;

            _repository.Update(user);
            await _repository.SaveChangesAsync();

            return _responseFactory.CreateResponse(
                true,
                "Profile updated successfully.",
                ActionType.Updated,
                user
            );
        }
        catch (Exception ex)
        {
            throw new ServiceException("Error updating profile.", ex);
        }
    }

    // 5. Delete User (only Admin or self)
    public async Task<IServiceResponse<bool>> DeleteUserAsync(Guid userId, string actingRole, Guid actingUserId)
    {
        try
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
            {
                return _responseFactory.CreateResponse<bool>(
                    false,
                    "User not found.",
                    ActionType.NotFound
                );
            }

            if (actingRole != "Admin" && user.Id != actingUserId)
            {
                return _responseFactory.CreateResponse<bool>(
                    false,
                    "Unauthorized to delete this user.",
                    ActionType.Unauthorized
                );
            }

            // Here you should have DeleteAsync in repository
            await _repository.DeleteAsync(user);
            await _repository.SaveChangesAsync();

            return _responseFactory.CreateResponse(
                true,
                "User deleted successfully.",
                ActionType.Deleted,
                true
            );
        }
        catch (Exception ex)
        {
            throw new ServiceException("Error deleting user.", ex);
        }
    }

    // 6. Get all users (Admin only)
    public async Task<IServiceResponse<IEnumerable<Users>>> GetAllUsersAsync(
        string actingRole,
        string? userId = null,
        string? name = null,
        string? email = null,
        string? phoneNo = null,
        string? role = null)
    {
        try
        {
            if (actingRole != "Admin")
            {
                return _responseFactory.CreateResponse<IEnumerable<Users>>(
                    false,
                    "Unauthorized.",
                    ActionType.Unauthorized
                );
            }

            var users = await _repository.GetFilteredAsync(userId, name, email, phoneNo, role);

            return _responseFactory.CreateResponse(
                true,
                "Users retrieved successfully.",
                ActionType.Retrieved,
                users
            );
        }
        catch (Exception ex)
        {
            throw new ServiceException("Error retrieving users.", ex);
        }
    }


    // 7. Get user by ID
    public async Task<IServiceResponse<Users>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
            {
                return _responseFactory.CreateResponse<Users>(
                    false,
                    "User not found.",
                    ActionType.NotFound
                );
            }

            return _responseFactory.CreateResponse(
                true,
                "User retrieved successfully.",
                ActionType.Retrieved,
                user
            );
        }
        catch (Exception ex)
        {
            throw new ServiceException("Error retrieving user.", ex);
        }
    }

    // 8. Admin Login
    public async Task<IServiceResponse<(Users User, string Token)>> LoginAdminAsync(string email, string password)
    {
        try
        {
            // Find user by email
            var user = await _repository.GetByEmailAsync(email);
            if (user == null)
            {
                return _responseFactory.CreateResponse<(Users User, string Token)>(
                    false,
                    "Admin account not found.",
                    ActionType.NotFound
                );
            }

            // Must be Admin
            if (user.Role != "Admin")
            {
                return _responseFactory.CreateResponse<(Users User, string Token)>(
                    false,
                    "Access denied: Not an Admin account.",
                    ActionType.Unauthorized
                );
            }

            if (!PasswordHelper.VerifyPassword(password, user.Password))
            {
                return _responseFactory.CreateResponse<(Users User, string Token)>(
                    false,
                    "Incorrect password.",
                    ActionType.Unauthorized
                );
            }

            var token = _jwtHelper.GenerateToken(user.Id, user.Email);

            return _responseFactory.CreateResponse(
                true,
                "Login successful.",
                ActionType.Retrieved,
                (user, token)
            );
        }
        catch (Exception ex)
        {
            throw new ServiceException("Error during admin login.", ex);
        }
    }

    // 9. Get Organizer (for filter)
    public async Task<IServiceResponse<IEnumerable<Users>>> GetOrganizersAsync(string actingRole)
    {
        try
        {
            if (actingRole != "Admin")
            {
                return _responseFactory.CreateResponse<IEnumerable<Users>>(
                    false,
                    "Unauthorized.",
                    ActionType.Unauthorized
                );
            }

            var organizers = await _repository.GetOrganizersAsync();

            return _responseFactory.CreateResponse(
                true,
                "Organizers retrieved successfully.",
                ActionType.Retrieved,
                organizers
            );
        }
        catch (Exception ex)
        {
            throw new ServiceException("Error retrieving organizers.", ex);
        }
    }

    // 10. Get Buyer (for filter)
    public async Task<IServiceResponse<IEnumerable<Users>>> GetBuyersAsync(string actingRole)
    {
        try
        {
            if (actingRole != "Admin")
            {
                return _responseFactory.CreateResponse<IEnumerable<Users>>(
                    false,
                    "Unauthorized.",
                    ActionType.Unauthorized
                );
            }

            var buyers = await _repository.GetBuyersAsync();

            return _responseFactory.CreateResponse(
                true,
                "Buyers retrieved successfully.",
                ActionType.Retrieved,
                buyers
            );
        }
        catch (Exception ex)
        {
            throw new ServiceException("Error retrieving buyers.", ex);
        }
    }
}
