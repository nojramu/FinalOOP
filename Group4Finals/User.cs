namespace SmartQuiz.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Student"; // "Student" or "Teacher"
        public string Bio { get; set; } = string.Empty;
        public string AlternativePassword { get; set; } = string.Empty; // For password recovery verification
        public string PhotoPath { get; set; } = string.Empty; // relative path under wwwroot
    }
}
