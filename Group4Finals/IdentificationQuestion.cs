using SmartQuiz.Models.Base;

namespace SmartQuiz.Models.QuestionTypes
{
    /// <summary>
    /// Identification Question implementation.
    /// Demonstrates: INHERITANCE - inherits from Question
    /// Demonstrates: POLYMORPHISM - implements IQuestionType with flexible behavior
    /// Demonstrates: ENCAPSULATION - private field with multiple acceptable answers
    /// </summary>
    public class IdentificationQuestion : Question, IQuestionType
    {
        private List<string> _acceptableAnswers = new();

        /// <summary>
        /// Encapsulation: Private field accessed through property
        /// Allows multiple acceptable answers (case-insensitive)
        /// </summary>
        public new string CorrectAnswer
        {
            get => _acceptableAnswers.Count > 0 ? _acceptableAnswers[0] : "";
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Identification questions must have at least one correct answer");
                }
                SetAcceptableAnswers(value);
            }
        }

        /// <summary>
        /// Encapsulation: Private method to handle answer parsing
        /// </summary>
        private void SetAcceptableAnswers(string answers)
        {
            _acceptableAnswers = answers.Split(',')
                .Select(a => a.Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .ToList();
        }

        public List<string> AcceptableAnswers => _acceptableAnswers.ToList(); // Read-only access

        public IdentificationQuestion()
        {
            Type = "Identification";
        }

        /// <summary>
        /// Polymorphism: Flexible validation that accepts multiple answers
        /// </summary>
        public bool ValidateAnswer(string studentAnswer)
        {
            if (string.IsNullOrWhiteSpace(studentAnswer))
                return false;

            return _acceptableAnswers.Any(answer =>
                answer.Equals(studentAnswer.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Polymorphism: Shows all acceptable answers
        /// </summary>
        public string GetCorrectAnswerDisplay()
        {
            return string.Join(", ", _acceptableAnswers);
        }

        public string GetQuestionTypeName() => "Identification";
    }
}

