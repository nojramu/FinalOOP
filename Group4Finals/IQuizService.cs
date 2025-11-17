using SmartQuiz.Models;
using System.Collections.Generic;

namespace SmartQuiz.Services.Interfaces
{
    /// <summary>
    /// Interface for Quiz Service operations.
    /// Demonstrates: ABSTRACTION - defines contract without implementation
    /// Demonstrates: LOW COUPLING - classes depend on interface, not concrete implementation
    /// </summary>
    public interface IQuizService
    {
        void AddQuiz(Quiz quiz);
        void UpdateQuiz(Quiz quiz);
        void DeleteQuiz(int id);
        Quiz? GetQuizById(int id);
        List<Quiz> GetPublishedQuizzes();
        List<Quiz> GetUnpublishedQuizzes();
        List<Quiz> GetAllQuizzes();
        List<Quiz> GetAllPublishedQuizzesForStudents();
        Quiz? GetQuizByIdForStudent(int quizId);
        Quiz? GetQuizByCodeForStudent(string quizCode);
    }
}

