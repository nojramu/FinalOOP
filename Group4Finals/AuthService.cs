using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;


namespace Group5.Services
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }


    public class AuthService
    {
        private readonly ConcurrentDictionary<string, User> _users = new();
        private readonly ILogger<AuthService> _logger;


        public AuthService(ILogger<AuthService> logger)
        {
            _logger = logger;
        }


        public async Task<User?> LoginUserAsync(string username, string password)
    {
            await Task.Delay(100); // Simulate async operation


            if (_users.TryGetValue(username, out var user) && user.Password == password)
            {
                _logger.LogInformation("User {Username} logged in successfully", username);
                return user;
            }


            _logger.LogWarning("Failed login attempt for username: {Username}", username);
            return null;
        }


        public async Task<bool> RegisterUserAsync(string username, string email, string password)
        {
            await Task.Delay(100); // Simulate async operation


            if (_users.ContainsKey(username))
            {
                _logger.LogWarning("Registration failed: Username {Username} already exists", username);
                return false;
            }


            if (_users.Values.Any(u => u.Email == email))
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", email);
                return false;
            }


            var newUser = new User
            {
                Username = username,
                Email = email,
                Password = password
            };


            _users.TryAdd(username, newUser);
            _logger.LogInformation("User {Username} registered successfully", username);
            return true;
        }
    }
}
