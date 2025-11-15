using BCrypt.Net;

namespace Group5.Services
{
    /// <summary>
    /// Centralized security helper class combining password, validation, time, and constants
    /// </summary>
    public static class SecurityHelper
    {
        #region Constants
        
        // Account lockout settings
        public const int MaxFailedLoginAttempts = 5;
        public const int LockoutDurationMinutes = 15;
        
        // Verification code settings
        public const int VerificationCodeExpirationMinutes = 10;
        public const int VerificationCodeMin = 100000;
        public const int VerificationCodeMax = 999999;
        
        // Password reset rate limiting
        public const int MaxPasswordResetRequestsPerHour = 3;
        public const int PasswordResetCooldownMinutes = 60;
        
        // Password requirements
        public const int MinPasswordLength = 6;
        
        #endregion

        #region Timezone Operations
        
        private static readonly TimeZoneInfo PhilippinesTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

        /// <summary>
        /// Gets the current time in Philippines timezone (UTC+8)
        /// </summary>
        public static DateTime GetPhilippinesTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PhilippinesTimeZone);
        }

        /// <summary>
        /// Converts a UTC DateTime to Philippines timezone
        /// </summary>
        public static DateTime ToPhilippinesTime(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, PhilippinesTimeZone);
        }
        
        #endregion

        #region Password Operations
        
        /// <summary>
        /// Hashes a password using BCrypt
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// Verifies a password against a hash
        /// Also handles plain text passwords for backward compatibility during migration
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            // Check if hash is BCrypt format (starts with $2a$, $2b$, or $2y$)
            if (hash.StartsWith("$2"))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, hash);
                }
                catch
                {
                    return false;
                }
            }
            
            // Backward compatibility: if not BCrypt format, treat as plain text (for migration)
            // This allows existing accounts to still login, but passwords should be migrated
            return password == hash;
        }

        /// <summary>
        /// Validates password strength
        /// Returns true if password meets requirements, false otherwise
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password cannot be empty.");

            if (password.Length < MinPasswordLength)
                return (false, $"Password must be at least {MinPasswordLength} characters.");

            // Check for at least one letter and one number
            bool hasLetter = password.Any(char.IsLetter);
            bool hasNumber = password.Any(char.IsDigit);

            if (!hasLetter)
                return (false, "Password must contain at least one letter.");

            if (!hasNumber)
                return (false, "Password must contain at least one number.");

            return (true, string.Empty);
        }
        
        #endregion

        #region Validation Operations
        
        /// <summary>
        /// Validates email format
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generates a random 6-digit verification code
        /// </summary>
        public static string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(VerificationCodeMin, VerificationCodeMax).ToString();
        }
        
        #endregion
    }
}

