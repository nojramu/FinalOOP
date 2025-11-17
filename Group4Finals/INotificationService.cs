using SmartQuiz.Models;
using System.Collections.Generic;

namespace SmartQuiz.Services.Interfaces
{
    /// <summary>
    /// Interface for Notification Service operations.
    /// Demonstrates: ABSTRACTION - defines contract for notification operations
    /// Demonstrates: LOW COUPLING - allows different implementations
    /// </summary>
    public interface INotificationService
    {
        List<Notification> GetStudentNotifications(int studentId);
        void CreateNotification(int studentId, string type, string title, string message, int? quizId = null, int? quizAttemptId = null, double? score = null);
        void MarkAsRead(int notificationId);
        bool DeleteNotification(int notificationId, int studentId);
    }
}

