namespace Group5.Services.Interfaces
{
    /// <summary>
    /// Interface for Email Service - Demonstrates ABSTRACTION principle
    /// Defines contract for email operations
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send verification code to email address
        /// Returns tuple with Success status and ErrorMessage
        /// </summary>
        Task<(bool Success, string ErrorMessage)> SendVerificationCodeAsync(string toEmail, string verificationCode);
        
        /// <summary>
        /// Send password reset code to email address
        /// Returns tuple with Success status and ErrorMessage
        /// </summary>
        Task<(bool Success, string ErrorMessage)> SendPasswordResetCodeAsync(string toEmail, string resetCode);
    }
}

