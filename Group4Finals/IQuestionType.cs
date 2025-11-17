namespace SmartQuiz.Models.Base
{
    /// <summary>
    /// Interface for different question types.
    /// Demonstrates: ABSTRACTION - defines contract for question behavior
    /// Demonstrates: POLYMORPHISM - allows different question types to be used interchangeably
    /// </summary>
    public interface IQuestionType
    {
        /// <summary>
        /// Validates the answer provided by the student
        /// </summary>
        bool ValidateAnswer(string studentAnswer);

        /// <summary>
        /// Gets the correct answer as a display string
        /// </summary>
        string GetCorrectAnswerDisplay();

        /// <summary>
        /// Gets the question type name
        /// </summary>
        string GetQuestionTypeName();
    }
}

