namespace WebAppNoAuth.Models;

public class AuthUser
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Location { get; set; }  = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // Parameterless constructor
    public AuthUser() { }

    // All-args constructor
    public AuthUser(string username, string email, string location, string department, string role)
    {
        Username = username;
        Email = email;
        Location = location;
        Department = department;
        Role = role;
    }
}
