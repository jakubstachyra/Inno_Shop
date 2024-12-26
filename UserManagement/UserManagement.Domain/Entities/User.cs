namespace UserManagement.Domain.Entities;

public class User : Base
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
    public required bool IsActive { get; set; } = false;
    public required string PasswordHash { get; set; }
    public required string ActivationToken { get; set; }
}
