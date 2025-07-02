using Eventpro.Domain.Models;
using Eventpro.Domain.ResponseFormat;

public interface IUserService
{
    Task<IServiceResponse<(Users User, string Token)>> CreateOrganizerProfileAsync(Users user);
    Task<IServiceResponse<(Users User, string Token)>> LoginOrganizerAsync(string email, string password);
    Task<IServiceResponse<(Users User, string Token)>> CreateUserProfileAsync(Users user);
    Task<IServiceResponse<(Users User, string Token)>> LoginUserAsync(string email, string password);
    Task<IServiceResponse<(Users User, string Token)>> LoginAdminAsync(string email, string password);

    Task<IServiceResponse<Users>> UpdateProfileAsync(Guid userId, Users updatedUser, string actingRole, Guid actingUserId);
    Task<IServiceResponse<bool>> DeleteUserAsync(Guid userId, string actingRole, Guid actingUserId);
    Task<IServiceResponse<IEnumerable<Users>>> GetAllUsersAsync(
        string actingRole,
        string? userId = null,
        string? name = null,
        string? email = null,
        string? phoneNo = null,
        string? role = null
    );
    Task<IServiceResponse<Users>> GetUserByIdAsync(Guid userId);
}
