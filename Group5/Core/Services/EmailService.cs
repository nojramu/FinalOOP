using System.Net;
using System.Net.Mail;
using Group5.Services.Interfaces;

namespace Group5.Services
{
    /// <summary>
    /// Email Service Implementation - Demonstrates POLYMORPHISM
    /// Implements IEmailService interface for sending emails
    /// Demonstrates ENCAPSULATION with private SMTP configuration fields
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUsername = configuration["EmailSettings:SmtpUsername"] ?? string.Empty;
            _smtpPassword = configuration["EmailSettings:SmtpPassword"] ?? string.Empty;
            _fromEmail = configuration["EmailSettings:FromEmail"] ?? _smtpUsername;
            _fromName = configuration["EmailSettings:FromName"] ?? "University of the East - Borrowing System";
        }

        public async Task<(bool Success, string ErrorMessage)> SendVerificationCodeAsync(string toEmail, string verificationCode)
        {
            try
            {
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    var errorMsg = "Email configuration not set. Please configure Gmail SMTP credentials in appsettings.json";
                    Console.WriteLine($"⚠️ {errorMsg}");
                    return (false, errorMsg);
                }

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = "Email Verification Code - University of the East",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; padding: 20px;'>
                            <h2 style='color: #8B0000;'>Email Verification</h2>
                            <p>Thank you for signing up with the University of the East Engineering Borrowing System.</p>
                            <p>Your verification code is:</p>
                            <div style='background-color: #f0f0f0; padding: 15px; border-radius: 5px; text-align: center; margin: 20px 0;'>
                                <h1 style='color: #8B0000; font-size: 32px; letter-spacing: 5px; margin: 0;'>{verificationCode}</h1>
                            </div>
                            <p>Please enter this code on the verification page to complete your registration.</p>
                            <p style='color: #666; font-size: 12px;'>This code will expire in 10 minutes.</p>
                            <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                            <p style='color: #999; font-size: 11px;'>If you did not request this code, please ignore this email.</p>
                        </body>
                        </html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                using var smtpClient = new SmtpClient(_smtpServer, _smtpPort);
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"Verification code sent to {toEmail}");
                return (true, string.Empty);
            }
            catch (SmtpException ex)
            {
                var errorMsg = $"SMTP Error: {ex.Message}. Please check your Gmail credentials and ensure you're using an App Password.";
                Console.WriteLine($"{errorMsg}");
                return (false, errorMsg);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error sending email: {ex.Message}";
                Console.WriteLine($"{errorMsg}");
                return (false, errorMsg);
            }
        }

        public async Task<(bool Success, string ErrorMessage)> SendPasswordResetCodeAsync(string toEmail, string resetCode)
        {
            try
            {
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    var errorMsg = "Email configuration not set. Please configure Gmail SMTP credentials in appsettings.json";
                    Console.WriteLine($"⚠️ {errorMsg}");
                    return (false, errorMsg);
                }

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = "Password Reset Code - University of the East",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; padding: 20px;'>
                            <h2 style='color: #8B0000;'>Password Reset</h2>
                            <p>You requested to reset your password for the University of the East Engineering Borrowing System.</p>
                            <p>Your password reset code is:</p>
                            <div style='background-color: #f0f0f0; padding: 15px; border-radius: 5px; text-align: center; margin: 20px 0;'>
                                <h1 style='color: #8B0000; font-size: 32px; letter-spacing: 5px; margin: 0;'>{resetCode}</h1>
                            </div>
                            <p>Please enter this code on the password reset page to create a new password.</p>
                            <p style='color: #666; font-size: 12px;'>This code will expire in 10 minutes.</p>
                            <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'/>
                            <p style='color: #999; font-size: 11px;'>If you did not request this code, please ignore this email and your password will remain unchanged.</p>
                        </body>
                        </html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                using var smtpClient = new SmtpClient(_smtpServer, _smtpPort);
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"✅ Password reset code sent to {toEmail}");
                return (true, string.Empty);
            }
            catch (SmtpException ex)
            {
                var errorMsg = $"SMTP Error: {ex.Message}. Please check your Gmail credentials and ensure you're using an App Password.";
                Console.WriteLine($"❌ {errorMsg}");
                return (false, errorMsg);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error sending email: {ex.Message}";
                Console.WriteLine($"❌ {errorMsg}");
                return (false, errorMsg);
            }
        }
    }
}

