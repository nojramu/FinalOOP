using SmartQuiz.Models.Base;

namespace SmartQuiz.Models.QuestionTypes
{
    /// <summary>
    /// True/False Question implementation.
    /// Demonstrates: INHERITANCE - inherits from Question
    /// Demonstrates: POLYMORPHISM - implements IQuestionType with different behavior
    /// Demonstrates: ENCAPSULATION - private field with validation
    /// </summary>
    public class TrueFalseQuestion : Question, IQuestionType
    {
        private string _correctAnswer = "";

        /// <summary>
        /// Encapsulation: Private field with validation in property setter
        /// </summary>
        public new string CorrectAnswer
        {
            get => _correctAnswer;
            set
            {
                if (value != null && (value.ToLower() == "true" || value.ToLower() == "false"))
                {
                    _correctAnswer = value.ToLower();
                }
                else
                {
                    throw new ArgumentException("True/False questions must have 'true' or 'false' as the correct answer");
                }
            }
        }

        public TrueFalseQuestion()
        {
            Type = "True or False";
            CorrectAnswer = "true";
        }

        /// <summary>
        /// Polymorphism: Different validation logic than MultipleChoiceQuestion
        /// </summary>
        public bool ValidateAnswer(string studentAnswer)
        {
            if (string.IsNullOrWhiteSpace(studentAnswer))
                return false;

            return studentAnswer.ToLower() == CorrectAnswer.ToLower();
        }

        /// <summary>
        /// Polymorphism: Different display format than MultipleChoiceQuestion
        /// </summary>
        public string GetCorrectAnswerDisplay()
        {
            return CorrectAnswer.Substring(0, 1).ToUpper() + CorrectAnswer.Substring(1);
        }

        public string GetQuestionTypeName() => "True or False";
    }
}

