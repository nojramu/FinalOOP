using SmartQuiz.Models;
using System.Collections.Generic;

namespace SmartQuiz.Services.Interfaces
{
    /// <summary>
    /// Interface for Quiz Attempt Service operations.
    /// Demonstrates: ABSTRACTION - defines contract for quiz attempt operations
    /// Demonstrates: LOW COUPLING - reduces dependency on concrete implementation
    /// </summary>
    public interface IQuizAttemptService
    {
        int SaveQuizAttempt(int quizId, int studentId, string studentName, Dictionary<int, string> selectedAnswers, Quiz quiz);
        List<QuizAttempt> GetQuizAttempts(int quizId);
        QuizAttempt? GetQuizAttempt(int attemptId, int quizId);
        List<QuizAttempt> GetStudentQuizAttempts(int studentId);
        bool HasStudentTakenQuiz(int quizId, int studentId);
        QuizAttempt? GetStudentQuizAttempt(int quizId, int studentId);
        void DeleteQuizAttempt(int attemptId, int quizId);
    }
}

