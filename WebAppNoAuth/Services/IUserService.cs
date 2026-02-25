using WebAppNoAuth.Models;

namespace WebAppNoAuth.Services;

public interface IUserService
{
    Task<AuthUser?> GetUserByUsernameAsync(string username);
    Task<List<AuthUser>> GetAllUsersAsync();
    Task<bool> CreateUserAsync(AuthUser user);
    Task<bool> UpdateUserAsync(AuthUser user);
    Task<bool> DeleteUserAsync(string username);
}