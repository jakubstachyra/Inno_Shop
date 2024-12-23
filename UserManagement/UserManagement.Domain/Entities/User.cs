namespace UserManagement.Domain.Entities;

public class User : Base
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; } = true;
    public string PasswordHash { get; set; }
}
