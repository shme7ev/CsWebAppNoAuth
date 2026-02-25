using WebAppNoAuth.Models;

namespace WebAppNoAuth.Services;

public class UserService : IUserService
{
    private static readonly List<AuthUser> _users = new()
    {
        new AuthUser("admin", "admin@company.com", "Headquarters", "IT", "Admin"),
        new AuthUser("john_doe", "john.doe@company.com", "New York", "Sales", "User"),
        new AuthUser("jane_smith", "jane.smith@company.com", "London", "Marketing", "Manager"),
        new AuthUser("bob_wilson", "bob.wilson@company.com", "Tokyo", "Engineering", "Developer"),
        new AuthUser("alice_brown", "alice.brown@company.com", "Sydney", "HR", "User")
    };

    public async Task<AuthUser?> GetUserByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;

        return await Task.FromResult( GetUser(username) );
    }

    public async Task<List<AuthUser>> GetAllUsersAsync()
    {
        return await Task.FromResult( _users.OrderBy(u => u.Username).ToList() );
    }

    public async Task<bool> CreateUserAsync(AuthUser user)
    {
        if (string.IsNullOrWhiteSpace(user.Username)) return false;

        var existingUser = GetUser(user.Username);

        if (existingUser != null) return false;

        _users.Add(user);
        return await Task.FromResult(true);
    }

    private static AuthUser? GetUser(string username) => _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

    public async Task<bool> UpdateUserAsync(AuthUser user)
    {
        if (string.IsNullOrWhiteSpace(user.Username)) return false;

        var existingUser = GetUser(user.Username);
        if (existingUser == null) return false;

        existingUser.Email = user.Email;
        existingUser.Location = user.Location;
        existingUser.Department = user.Department;
        existingUser.Role = user.Role;

        return await Task.FromResult(true);
    }

    public async Task<bool> DeleteUserAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;

        var user = GetUser(username);
        if (user == null) return false;

        _users.Remove(user);
        return await Task.FromResult(true);
    }
}
