using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartQuiz.Data;
using SmartQuiz.Models;

namespace SmartQuiz.Services
{
    public class NotificationService
    {
        private readonly DbContextFactory _dbContextFactory;

        public NotificationService(DbContextFactory dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Gets all notifications for a specific student
        /// </summary>
        public List<Notification> GetStudentNotifications(int studentId)
        {
            try
            {
                using var context = _dbContextFactory.GetMasterDbContext();
                context.Database.EnsureCreated();
                
                var notifications = context.Notifications
                    .Where(n => n.StudentId == studentId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();

                Console.WriteLine($"GetStudentNotifications: Found {notifications.Count} notifications for student {studentId}");
                return notifications;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting student notifications: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<Notification>();
            }
        }

        /// <summary>
        /// Creates a new notification
        /// </summary>
        public void CreateNotification(int studentId, string type, string title, string message, int? quizId = null, int? quizAttemptId = null, double? score = null)
        {
            try
            {
                using var context = _dbContextFactory.GetMasterDbContext();
                context.Database.EnsureCreated();
                
                var notification = new Notification
                {
                    StudentId = studentId,
                    Type = type,
                    Title = title,
                    Message = message,
                    QuizId = quizId,
                    QuizAttemptId = quizAttemptId,
                    Score = score,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                context.Notifications.Add(notification);
                context.SaveChanges();
                
                Console.WriteLine($"Created notification for student {studentId}: {title}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating notification: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        public void MarkAsRead(int notificationId)
        {
            try
            {
                using var context = _dbContextFactory.GetMasterDbContext();
                context.Database.EnsureCreated();
                
                var notification = context.Notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking notification as read: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a notification
        /// </summary>
        public void DeleteNotification(int notificationId)
        {
            try
            {
                using var context = _dbContextFactory.GetMasterDbContext();
                context.Database.EnsureCreated();
                
                var notification = context.Notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    context.Notifications.Remove(notification);
                    context.SaveChanges();
                    Console.WriteLine($"Deleted notification {notificationId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting notification: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
