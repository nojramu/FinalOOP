using SmartQuiz.Models;
using SmartQuiz.Models.Base;

namespace SmartQuiz.Models.QuestionTypes
{
    /// <summary>
    /// Factory pattern for creating question instances.
    /// Demonstrates: POLYMORPHISM - returns different implementations through common interface
    /// Demonstrates: ENCAPSULATION - hides creation logic from clients
    /// Demonstrates: COHESION - single responsibility for question creation
    /// </summary>
    public static class QuestionFactory
    {
        /// <summary>
        /// Creates a question instance based on type.
        /// Demonstrates: POLYMORPHISM - returns IQuestionType interface
        /// </summary>
        public static IQuestionType CreateQuestionType(Question question)
        {
            if (question == null)
                throw new ArgumentNullException(nameof(question));

            return question.Type switch
            {
                "Multiple Choice" => CreateMultipleChoiceQuestion(question),
                "True or False" => CreateTrueFalseQuestion(question),
                "Identification" => CreateIdentificationQuestion(question),
                _ => throw new ArgumentException($"Unknown question type: {question.Type}")
            };
        }

        /// <summary>
        /// Encapsulation: Private method hides creation details
        /// </summary>
        private static MultipleChoiceQuestion CreateMultipleChoiceQuestion(Question question)
        {
            var mcq = new MultipleChoiceQuestion
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                Type = question.Type,
                Options = question.Options,
                CorrectAnswerIndex = question.CorrectAnswerIndex,
                TimeLimit = question.TimeLimit,
                QuizId = question.QuizId
            };
            return mcq;
        }

        private static TrueFalseQuestion CreateTrueFalseQuestion(Question question)
        {
            var tfq = new TrueFalseQuestion
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                Type = question.Type,
                CorrectAnswer = question.CorrectAnswer,
                TimeLimit = question.TimeLimit,
                QuizId = question.QuizId
            };
            return tfq;
        }

        private static IdentificationQuestion CreateIdentificationQuestion(Question question)
        {
            var idq = new IdentificationQuestion
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                Type = question.Type,
                CorrectAnswer = question.CorrectAnswer,
                TimeLimit = question.TimeLimit,
                QuizId = question.QuizId
            };
            return idq;
        }
    }
}

