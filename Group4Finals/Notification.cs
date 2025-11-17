using System;
using System.ComponentModel.DataAnnotations;

namespace SmartQuiz.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        
        public int StudentId { get; set; } // The student who should see this notification
        
        public string Type { get; set; } = ""; // "NewQuiz" or "QuizScore"
        
        public string Title { get; set; } = ""; // Notification title
        
        public string Message { get; set; } = ""; // Notification message
        
        public int? QuizId { get; set; } // Related quiz ID (if applicable)
        
        public int? QuizAttemptId { get; set; } // Related quiz attempt ID (for score notifications)
        
        public double? Score { get; set; } // Score percentage (for score notifications)
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRead { get; set; } = false; // Whether the student has seen this notification
    }
}

