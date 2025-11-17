using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartQuiz.Data;
using SmartQuiz.Models;

namespace SmartQuiz.Services
{
    public class QuizAttemptService
    {
        private readonly DbContextFactory _dbContextFactory;
        private readonly UserSessionService _userSession;

        public QuizAttemptService(DbContextFactory dbContextFactory, UserSessionService userSession)
        {
            _dbContextFactory = dbContextFactory;
            _userSession = userSession;
        }

        /// <summary>
        /// Ensures QuizAttempts and StudentAnswers tables exist in the database
        /// </summary>
        private void EnsureQuizAttemptTablesExist(AppDbContext context)
        {
            try
            {
                var connection = context.Database.GetDbConnection();
                var wasOpen = connection.State == System.Data.ConnectionState.Open;
                
                if (!wasOpen)
                {
                    connection.Open();
                }

                try
                {
                    // Check if QuizAttempts table exists
                    using var checkCommand = connection.CreateCommand();
                    checkCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='QuizAttempts'";
                    var quizAttemptsExists = checkCommand.ExecuteScalar() != null;

                    if (!quizAttemptsExists)
                    {
                        Console.WriteLine("Creating QuizAttempts and StudentAnswers tables...");
                        
                        // Create QuizAttempts table
                        using var createQuizAttemptsCommand = connection.CreateCommand();
                        createQuizAttemptsCommand.CommandText = @"
                            CREATE TABLE IF NOT EXISTS QuizAttempts (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                QuizId INTEGER NOT NULL,
                                StudentId INTEGER NOT NULL,
                                StudentName TEXT,
                                SubmittedAt TEXT NOT NULL,
                                CorrectAnswers INTEGER NOT NULL,
                                TotalQuestions INTEGER NOT NULL,
                                Score REAL NOT NULL
                            )";
                        createQuizAttemptsCommand.ExecuteNonQuery();
                        Console.WriteLine("QuizAttempts table created.");

                        // Create StudentAnswers table
                        using var createStudentAnswersCommand = connection.CreateCommand();
                        createStudentAnswersCommand.CommandText = @"
                            CREATE TABLE IF NOT EXISTS StudentAnswers (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                QuizAttemptId INTEGER NOT NULL,
                                QuestionId INTEGER NOT NULL,
                                AnswerText TEXT,
                                IsCorrect INTEGER NOT NULL,
                                FOREIGN KEY (QuizAttemptId) REFERENCES QuizAttempts(Id) ON DELETE CASCADE
                            )";
                        createStudentAnswersCommand.ExecuteNonQuery();
                        Console.WriteLine("StudentAnswers table created.");
                    }
                    else
                    {
                        Console.WriteLine("QuizAttempts and StudentAnswers tables already exist.");
                    }
                }
                finally
                {
                    if (!wasOpen)
                    {
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring QuizAttempt tables exist: {ex.Message}");
                // Don't throw - let EnsureCreated handle it
            }
        }

        /// <summary>
        /// Gets the teacher's database context for a specific quiz
        /// </summary>
        private AppDbContext GetTeacherContext(int quizId)
        {
            // Use shared quiz database
            var quizContext = _dbContextFactory.GetQuizDbContext();
            // Ensure database and tables exist (including QuizAttempts and StudentAnswers)
            quizContext.Database.EnsureCreated();
            // Manually ensure QuizAttempts and StudentAnswers tables exist
            EnsureQuizAttemptTablesExist(quizContext);
            
            var quiz = quizContext.Quizzes.FirstOrDefault(q => q.Id == quizId);
            if (quiz == null)
            {
                throw new Exception($"Quiz {quizId} not found");
            }
            
            return quizContext;
        }

        /// <summary>
        /// Saves a quiz attempt (student submission)
        /// </summary>
        public int SaveQuizAttempt(int quizId, int studentId, string studentName, Dictionary<int, string> selectedAnswers, Quiz quiz)
        {
            try
            {
                using var context = GetTeacherContext(quizId);
                
                // Calculate score
                int correctAnswers = 0;
                int totalQuestions = quiz?.Questions?.Count ?? 0;
                var studentAnswerList = new List<StudentAnswer>();

                if (quiz?.Questions != null)
                {
                    Console.WriteLine($"SaveQuizAttempt: Processing {quiz.Questions.Count} questions");
                    Console.WriteLine($"SaveQuizAttempt: selectedAnswers contains {selectedAnswers.Count} entries");
                    foreach (var kvp in selectedAnswers)
                    {
                        Console.WriteLine($"  selectedAnswers[{kvp.Key}] = '{kvp.Value}'");
                    }
                    
                    foreach (var question in quiz.Questions)
                    {
                        var hasAnswer = selectedAnswers.ContainsKey(question.Id);
                        var answerText = hasAnswer ? selectedAnswers[question.Id] : null;
                        
                        // Ensure answerText is not empty string - convert to null if empty
                        if (string.IsNullOrWhiteSpace(answerText))
                        {
                            answerText = null;
                        }
                        
                        var studentAnswer = new StudentAnswer
                        {
                            QuestionId = question.Id,
                            AnswerText = answerText,
                            IsCorrect = false
                        };

                        if (hasAnswer && !string.IsNullOrEmpty(answerText))
                        {
                            var answer = answerText;
                            Console.WriteLine($"SaveQuizAttempt: Question {question.Id} ({question.Type}): Answer='{answer}'");
                            
                            if (question.Type == "Multiple Choice")
                            {
                                if (int.TryParse(answer, out int selectedIndex) && selectedIndex == question.CorrectAnswerIndex)
                                {
                                    correctAnswers++;
                                    studentAnswer.IsCorrect = true;
                                    Console.WriteLine($"  -> Correct! Selected index {selectedIndex} matches correct index {question.CorrectAnswerIndex}");
                                }
                                else
                                {
                                    Console.WriteLine($"  -> Incorrect. Selected index {selectedIndex}, correct index {question.CorrectAnswerIndex}");
                                }
                            }
                            else if (question.Type == "True or False" || question.Type == "Identification")
                            {
                                if (answer.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
                                {
                                    correctAnswers++;
                                    studentAnswer.IsCorrect = true;
                                    Console.WriteLine($"  -> Correct! Answer '{answer}' matches '{question.CorrectAnswer}'");
                                }
                                else
                                {
                                    Console.WriteLine($"  -> Incorrect. Answer '{answer}' does not match '{question.CorrectAnswer}'");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"SaveQuizAttempt: Question {question.Id}: No answer provided (hasAnswer={hasAnswer}, answerText='{answerText ?? "null"}')");
                        }

                        studentAnswerList.Add(studentAnswer);
                        Console.WriteLine($"  -> Saved StudentAnswer: QuestionId={studentAnswer.QuestionId}, AnswerText='{studentAnswer.AnswerText ?? "null"}', IsCorrect={studentAnswer.IsCorrect}");
                    }
                }

                double score = totalQuestions > 0 ? (correctAnswers * 100.0 / totalQuestions) : 0;

                // Ensure QuizAttempts and StudentAnswers tables exist
                context.Database.EnsureCreated();
                // Manually ensure QuizAttempts and StudentAnswers tables exist (for existing databases)
                EnsureQuizAttemptTablesExist(context);
                
                // Create quiz attempt
                var attempt = new QuizAttempt
                {
                    QuizId = quizId,
                    StudentId = studentId,
                    StudentName = studentName,
                    SubmittedAt = DateTime.UtcNow,
                    CorrectAnswers = correctAnswers,
                    TotalQuestions = totalQuestions,
                    Score = score,
                    Answers = studentAnswerList
                };

                Console.WriteLine($"SaveQuizAttempt: Adding attempt to context. QuizId={quizId}, StudentId={studentId}, Answers.Count={studentAnswerList.Count}");
                context.QuizAttempts.Add(attempt);
                
                Console.WriteLine($"SaveQuizAttempt: Calling SaveChanges...");
                var changes = context.SaveChanges();
                Console.WriteLine($"SaveQuizAttempt: SaveChanges completed. Changes saved: {changes}");

                Console.WriteLine($"Quiz attempt saved: QuizId={quizId}, StudentId={studentId}, AttemptId={attempt.Id}, Score={score:F1}%");
                return attempt.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving quiz attempt: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Gets all quiz attempts for a specific quiz (for teachers to view)
        /// </summary>
        public List<QuizAttempt> GetQuizAttempts(int quizId)
        {
            try
            {
                using var context = GetTeacherContext(quizId);
                
                var attempts = context.QuizAttempts
                    .Include(qa => qa.Answers)
                    .Where(qa => qa.QuizId == quizId)
                    .OrderByDescending(qa => qa.SubmittedAt)
                    .ToList();

                return attempts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting quiz attempts: {ex.Message}");
                return new List<QuizAttempt>();
            }
        }

        /// <summary>
        /// Gets a specific quiz attempt by ID
        /// </summary>
        public QuizAttempt? GetQuizAttempt(int attemptId, int quizId)
        {
            try
            {
                using var context = GetTeacherContext(quizId);
                
                var attempt = context.QuizAttempts
                    .Include(qa => qa.Answers)
                    .FirstOrDefault(qa => qa.Id == attemptId && qa.QuizId == quizId);

                if (attempt != null)
                {
                    Console.WriteLine($"GetQuizAttempt: Found attempt {attemptId} with {attempt.Answers?.Count ?? 0} answers");
                    if (attempt.Answers != null)
                    {
                        foreach (var answer in attempt.Answers)
                        {
                            Console.WriteLine($"  Answer: QuestionId={answer.QuestionId}, AnswerText='{answer.AnswerText ?? "null"}', IsCorrect={answer.IsCorrect}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"GetQuizAttempt: Attempt {attemptId} not found");
                }

                return attempt;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting quiz attempt: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets all quiz attempts for a specific student (for students to view their history)
        /// </summary>
        public List<QuizAttempt> GetStudentQuizAttempts(int studentId)
        {
            try
            {
                // Use shared quiz database
                using var quizContext = _dbContextFactory.GetQuizDbContext();
                // Ensure database and tables exist (including QuizAttempts and StudentAnswers)
                quizContext.Database.EnsureCreated();
                // Manually ensure QuizAttempts and StudentAnswers tables exist
                EnsureQuizAttemptTablesExist(quizContext);
                
                var attempts = quizContext.QuizAttempts
                    .Include(qa => qa.Answers)
                    .Where(qa => qa.StudentId == studentId)
                    .OrderByDescending(qa => qa.SubmittedAt)
                    .ToList();

                Console.WriteLine($"GetStudentQuizAttempts: Found {attempts.Count} attempts for student {studentId}");
                return attempts;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting student quiz attempts: {ex.Message}");
                return new List<QuizAttempt>();
            }
        }

        /// <summary>
        /// Checks if a student has already taken a specific quiz
        /// </summary>
        public bool HasStudentTakenQuiz(int quizId, int studentId)
        {
            try
            {
                using var context = GetTeacherContext(quizId);
                // Manually ensure QuizAttempts and StudentAnswers tables exist
                EnsureQuizAttemptTablesExist(context);
                
                var hasTaken = context.QuizAttempts
                    .Any(qa => qa.QuizId == quizId && qa.StudentId == studentId);

                Console.WriteLine($"HasStudentTakenQuiz: QuizId={quizId}, StudentId={studentId}, HasTaken={hasTaken}");
                return hasTaken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking if student has taken quiz: {ex.Message}");
                return false; // If there's an error, allow access (fail open)
            }
        }

        /// <summary>
        /// Gets the quiz attempt for a specific student and quiz (if they've taken it)
        /// </summary>
        public QuizAttempt? GetStudentQuizAttempt(int quizId, int studentId)
        {
            try
            {
                using var context = GetTeacherContext(quizId);
                // Manually ensure QuizAttempts and StudentAnswers tables exist
                EnsureQuizAttemptTablesExist(context);
                
                var attempt = context.QuizAttempts
                    .Include(qa => qa.Answers)
                    .Where(qa => qa.QuizId == quizId && qa.StudentId == studentId)
                    .OrderByDescending(qa => qa.SubmittedAt)
                    .FirstOrDefault();

                return attempt;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting student quiz attempt: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deletes a quiz attempt by ID
        /// </summary>
        public void DeleteQuizAttempt(int attemptId, int quizId)
        {
            try
            {
                using var context = GetTeacherContext(quizId);
                // Manually ensure QuizAttempts and StudentAnswers tables exist
                EnsureQuizAttemptTablesExist(context);
                
                var attempt = context.QuizAttempts
                    .Include(qa => qa.Answers)
                    .FirstOrDefault(qa => qa.Id == attemptId && qa.QuizId == quizId);

                if (attempt != null)
                {
                    context.QuizAttempts.Remove(attempt);
                    context.SaveChanges();
                    Console.WriteLine($"Deleted quiz attempt {attemptId} for quiz {quizId}");
                }
                else
                {
                    throw new Exception($"Quiz attempt {attemptId} not found for quiz {quizId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting quiz attempt: {ex.Message}");
                throw;
            }
        }
    }
}

