using System.ComponentModel.DataAnnotations;

namespace SmartQuiz.Models
{
    public class StudentAnswer
    {
        [Key]
        public int Id { get; set; }
        
        public int QuizAttemptId { get; set; } // Foreign key to QuizAttempt
        public int QuestionId { get; set; } // Which question was answered
        
        public string? AnswerText { get; set; } // The student's answer
        public bool IsCorrect { get; set; } // Whether the answer was correct
        
        // Navigation property
        public QuizAttempt? QuizAttempt { get; set; }
    }
}

