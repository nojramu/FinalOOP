using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartQuiz.Models
{
    public class QuizAttempt
    {
        [Key]
        public int Id { get; set; }
        
        public int QuizId { get; set; } // Which quiz was taken
        public int StudentId { get; set; } // Which student took it
        public string? StudentName { get; set; } // Student's name (for display)
        
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public double Score { get; set; } // Percentage score
        
        // Navigation property for individual answers
        public List<StudentAnswer> Answers { get; set; } = new();
    }
}

