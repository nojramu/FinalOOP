using SmartQuiz.Models.Base;

namespace SmartQuiz.Models.QuestionTypes
{
    /// <summary>
    /// Multiple Choice Question implementation.
    /// Demonstrates: INHERITANCE - inherits from Question
    /// Demonstrates: POLYMORPHISM - implements IQuestionType interface
    /// Demonstrates: ENCAPSULATION - private fields with controlled access through properties
    /// </summary>
    public class MultipleChoiceQuestion : Question, IQuestionType
    {
        private int _correctAnswerIndex = -1;
        private List<string> _options = new();

        /// <summary>
        /// Encapsulation: Private field accessed through property with validation
        /// </summary>
        public int CorrectAnswerIndex
        {
            get => _correctAnswerIndex;
            set
            {
                if (value < 0 || (Options != null && value >= Options.Count))
                {
                    throw new ArgumentException("Correct answer index is out of range");
                }
                _correctAnswerIndex = value;
            }
        }

        /// <summary>
        /// Encapsulation: Controlled access to options list
        /// </summary>
        public new List<string> Options
        {
            get => _options;
            set
            {
                if (value == null || value.Count < 2)
                {
                    throw new ArgumentException("Multiple choice questions must have at least 2 options");
                }
                _options = value;
            }
        }

        public MultipleChoiceQuestion()
        {
            Type = "Multiple Choice";
            Options = new List<string>();
        }

        /// <summary>
        /// Polymorphism: Implements interface method with specific behavior
        /// </summary>
        public bool ValidateAnswer(string studentAnswer)
        {
            if (int.TryParse(studentAnswer, out int selectedIndex))
            {
                return selectedIndex == CorrectAnswerIndex;
            }
            return false;
        }

        /// <summary>
        /// Polymorphism: Provides specific implementation for display
        /// </summary>
        public string GetCorrectAnswerDisplay()
        {
            if (CorrectAnswerIndex >= 0 && CorrectAnswerIndex < Options.Count)
            {
                char letter = (char)('A' + CorrectAnswerIndex);
                return $"{letter}: {Options[CorrectAnswerIndex]}";
            }
            return "Invalid answer";
        }

        public string GetQuestionTypeName() => "Multiple Choice";

        /// <summary>
        /// Cohesion: Well-focused method for this specific question type
        /// </summary>
        public void AddOption(string option)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                throw new ArgumentException("Option cannot be empty");
            }
            if (Options.Count >= 4)
            {
                throw new InvalidOperationException("Multiple choice questions can have at most 4 options");
            }
            Options.Add(option);
        }
    }
}

