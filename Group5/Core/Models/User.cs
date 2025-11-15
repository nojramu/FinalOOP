namespace Group5.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsEmailVerified { get; set; }
        
        // Account lockout fields
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockedUntil { get; set; } = null;
    }
}