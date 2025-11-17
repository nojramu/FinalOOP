using Microsoft.EntityFrameworkCore;
using SmartQuiz.Models;

namespace SmartQuiz.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ✅ Your Users table
        public DbSet<User> Users => Set<User>();
        
        // ✅ Quiz tables
        public DbSet<Quiz> Quizzes => Set<Quiz>();
        public DbSet<Question> Questions => Set<Question>();
        
        // ✅ Quiz attempt tables (for storing student submissions)
        public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
        public DbSet<StudentAnswer> StudentAnswers => Set<StudentAnswer>();
        
        // ✅ Notifications table
        public DbSet<Notification> Notifications => Set<Notification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

               // Configure Quiz-Question relationship
               modelBuilder.Entity<Question>()
                   .HasOne(q => q.Quiz)
                   .WithMany(quiz => quiz.Questions)
                   .HasForeignKey(q => q.QuizId)
                   .OnDelete(DeleteBehavior.Cascade);
               
               // Configure QuizAttempt-StudentAnswer relationship
               modelBuilder.Entity<StudentAnswer>()
                   .HasOne(sa => sa.QuizAttempt)
                   .WithMany(qa => qa.Answers)
                   .HasForeignKey(sa => sa.QuizAttemptId)
                   .OnDelete(DeleteBehavior.Cascade);
               
               // Configure Quiz-User relationship (optional, for future use)
               // Note: We're not adding a navigation property to avoid circular references
        }
    }
}
