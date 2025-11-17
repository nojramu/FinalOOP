using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartQuiz.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string QuestionText { get; set; } = "";
        
        public string Type { get; set; } = "Multiple Choice"; // Multiple Choice, True or False, Identification
        
        // Store options as JSON string for SQLite
        [Column(TypeName = "TEXT")]
        public string OptionsJson { get; set; } = "[]"; // JSON array of options
        
        [NotMapped]
        private List<string>? _options;
        
        [NotMapped]
        public List<string> Options
        {
            get
            {
                if (_options == null)
                {
                    if (string.IsNullOrEmpty(OptionsJson) || OptionsJson == "[]")
                        _options = new List<string>();
                    else
                        _options = System.Text.Json.JsonSerializer.Deserialize<List<string>>(OptionsJson) ?? new List<string>();
                }
                return _options;
            }
            set
            {
                _options = value ?? new List<string>();
                OptionsJson = System.Text.Json.JsonSerializer.Serialize(_options);
            }
        }
        
        // Method to sync Options back to OptionsJson when modified
        public void SyncOptionsToJson()
        {
            if (_options != null)
            {
                OptionsJson = System.Text.Json.JsonSerializer.Serialize(_options);
            }
        }
        
        public int CorrectAnswerIndex { get; set; } = -1; // For Multiple Choice
        public string CorrectAnswer { get; set; } = ""; // For True/False and Identification
        public int? TimeLimit { get; set; } // Per-question time limit in seconds (optional)
        
        [NotMapped]
        public bool HasQuestionTimer { get; set; } // UI helper property to track if timer is enabled
        
        // Foreign key to Quiz
        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }
    }
}

