using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartQuiz.Models
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? QuizCode { get; set; }
        public bool IsPrivate { get; set; } = false; // Whether the quiz requires a code (private) or is public
        public int TotalTimeLimit { get; set; } = 60; // Total quiz time limit in minutes
        public bool IsPublished { get; set; } = false; // Whether the quiz is published
        public int UserId { get; set; } // Foreign key to User who created this quiz
        
        // Navigation property for questions
        public List<Question> Questions { get; set; } = new();
    }
}

