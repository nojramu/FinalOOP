using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartQuiz.Data;
using SmartQuiz.Models;

namespace SmartQuiz.Services
{
    public class QuizService
    {
        private readonly DbContextFactory _dbContextFactory;
        private readonly UserSessionService _userSession;

        public QuizService(DbContextFactory dbContextFactory, UserSessionService userSession)
        {
            _dbContextFactory = dbContextFactory;
            _userSession = userSession;
        }

        /// <summary>
        /// Gets the user-specific database context
        /// </summary>
        private AppDbContext GetUserContext()
        {
            var isLoggedIn = _userSession.IsLoggedIn;
            var currentUserId = _userSession.CurrentUserId;
            
            if (!isLoggedIn || !currentUserId.HasValue)
            {
                Console.WriteLine($"GetUserContext: IsLoggedIn={isLoggedIn}, CurrentUserId={currentUserId}");
                throw new UnauthorizedAccessException($"User must be logged in to access quizzes. IsLoggedIn={isLoggedIn}, CurrentUserId={currentUserId}");
            }

            // GetUserDbContext already calls EnsureCreated, so we don't need to call it again
            var context = _dbContextFactory.GetUserDbContext();
            
            // Ensure IsPrivate column exists before any queries
            EnsureIsPrivateColumnExists(context, currentUserId.Value);
            
            return context;
        }

        private void EnsureIsPrivateColumnExists(AppDbContext context, int userId)
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
                    // Check if Quizzes table exists
                    using var tableCheckCommand = connection.CreateCommand();
                    tableCheckCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Quizzes'";
                    var tableExists = tableCheckCommand.ExecuteScalar() != null;
                    
                    if (tableExists)
                    {
                        // Check if IsPrivate column exists
                        using var checkCommand = connection.CreateCommand();
                        checkCommand.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Quizzes') WHERE name='IsPrivate'";
                        var result = checkCommand.ExecuteScalar();
                        var columnExists = result != null && Convert.ToInt32(result) > 0;

                        if (!columnExists)
                        {
                            Console.WriteLine($"QuizService: Adding IsPrivate column to Quizzes table in user database {userId}...");
                            using var alterCommand = connection.CreateCommand();
                            alterCommand.CommandText = "ALTER TABLE Quizzes ADD COLUMN IsPrivate INTEGER NOT NULL DEFAULT 0";
                            alterCommand.ExecuteNonQuery();
                            Console.WriteLine($"QuizService: IsPrivate column added successfully to user database {userId}");
                            
                            // Verify the column was added
                            using var verifyCommand = connection.CreateCommand();
                            verifyCommand.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Quizzes') WHERE name='IsPrivate'";
                            var verifyResult = verifyCommand.ExecuteScalar();
                            var verified = verifyResult != null && Convert.ToInt32(verifyResult) > 0;
                            Console.WriteLine($"QuizService: IsPrivate column verification: {verified}");
                        }
                        else
                        {
                            Console.WriteLine($"QuizService: IsPrivate column already exists in user database {userId}");
                        }
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
                Console.WriteLine($"QuizService: Error ensuring IsPrivate column for user {userId}: {ex.Message}");
                Console.WriteLine($"QuizService: Stack trace: {ex.StackTrace}");
                // Don't throw - let it fail naturally if column is missing
            }
        }

        public void AddQuiz(Quiz quiz)
        {
            try
            {
                using var context = GetUserContext();
                
                // Ensure IsPrivate column exists before adding quiz
                var currentUserId = _userSession.CurrentUserId ?? 0;
                Console.WriteLine($"AddQuiz: Ensuring IsPrivate column exists for user {currentUserId}...");
                EnsureIsPrivateColumnExists(context, currentUserId);
                
                // Verify database connection is open
                var dbConnection = context.Database.GetDbConnection();
                if (dbConnection.State != System.Data.ConnectionState.Open)
                {
                    dbConnection.Open();
                    Console.WriteLine($"AddQuiz: Opened database connection");
                }
                Console.WriteLine($"AddQuiz: Database connection state: {dbConnection.State}, ConnectionString: {dbConnection.ConnectionString}");
                
                // Set the UserId to the current logged-in user
                quiz.UserId = currentUserId;
                Console.WriteLine($"AddQuiz: Adding quiz with UserId={quiz.UserId}, Questions.Count={quiz.Questions?.Count ?? 0}, IsPrivate={quiz.IsPrivate}, Title={quiz.Title}");
                
                // Clear question IDs so they get new ones from the database
                if (quiz.Questions == null)
                {
                    quiz.Questions = new List<Question>();
                }
                
                foreach (var question in quiz.Questions)
                {
                    question.Id = 0;
                    question.QuizId = 0; // Will be set when quiz is saved
                    question.SyncOptionsToJson();
                    Console.WriteLine($"AddQuiz: Question '{question.QuestionText}' (Type: {question.Type})");
                }
                
                Console.WriteLine($"AddQuiz: About to add quiz to context. Quiz properties: Id={quiz.Id}, Title={quiz.Title}, IsPrivate={quiz.IsPrivate}, UserId={quiz.UserId}");
                context.Quizzes.Add(quiz);
                
                Console.WriteLine($"AddQuiz: Quiz added to context. About to call SaveChanges...");
                var changes = context.SaveChanges();
                Console.WriteLine($"AddQuiz: SaveChanges returned {changes} changes. Quiz ID after save: {quiz.Id}");
                
                // Verify quiz was saved
                if (quiz.Id > 0)
                {
                    var savedQuiz = context.Quizzes.FirstOrDefault(q => q.Id == quiz.Id);
                    if (savedQuiz != null)
                    {
                        Console.WriteLine($"AddQuiz: ✅ Verified quiz {quiz.Id} exists in database: {savedQuiz.Title}, IsPrivate={savedQuiz.IsPrivate}");
                    }
                    else
                    {
                        Console.WriteLine($"AddQuiz: ❌ ERROR - Quiz {quiz.Id} not found in database after save!");
                        
                        // Try to find all quizzes to see what's in the database
                        var allQuizzes = context.Quizzes.ToList();
                        Console.WriteLine($"AddQuiz: Database currently has {allQuizzes.Count} quizzes");
                        foreach (var q in allQuizzes)
                        {
                            Console.WriteLine($"AddQuiz: Found quiz in DB: Id={q.Id}, Title={q.Title}, UserId={q.UserId}, IsPrivate={q.IsPrivate}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"AddQuiz: ❌ ERROR - Quiz ID is 0 after SaveChanges! Quiz was not saved properly.");
                    
                    // Check database connection
                    var dbConnectionCheck = context.Database.GetDbConnection();
                    Console.WriteLine($"AddQuiz: Database connection state: {dbConnectionCheck.State}");
                    Console.WriteLine($"AddQuiz: Database connection string: {dbConnectionCheck.ConnectionString}");
                }
                
                // Set QuizId for all questions after quiz is saved
                foreach (var question in quiz.Questions)
                {
                    question.QuizId = quiz.Id;
                }
                
                // Save again to update QuizId for questions
                if (quiz.Questions.Count > 0)
                {
                    var questionChanges = context.SaveChanges();
                    Console.WriteLine($"AddQuiz: SaveChanges for questions returned {questionChanges} changes");
                    
                    // Verify questions were saved
                    var savedQuestions = context.Questions.Where(q => q.QuizId == quiz.Id).ToList();
                    Console.WriteLine($"AddQuiz: Verified {savedQuestions.Count} questions in database for quiz {quiz.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding quiz: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Log inner exception if available
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner stack trace: {ex.InnerException.StackTrace}");
                }
                
                throw;
            }
        }

        public void UpdateQuiz(Quiz quiz)
        {
            try
            {
                using var context = GetUserContext();
                var currentUserId = _userSession.CurrentUserId ?? 0;
                
                // Filter by Id and UserId to ensure users can only update their own quizzes
                var existing = context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefault(q => q.Id == quiz.Id && q.UserId == currentUserId);
                
                if (existing != null)
                {
                    Console.WriteLine($"Updating existing quiz {quiz.Id} with {quiz.Questions.Count} questions");
                    
                    existing.Title = quiz.Title;
                    existing.Description = quiz.Description;
                    existing.QuizCode = quiz.QuizCode;
                    existing.IsPrivate = quiz.IsPrivate;
                    existing.TotalTimeLimit = quiz.TotalTimeLimit;
                    existing.IsPublished = quiz.IsPublished;
                    // UserId should not change
                    
                    // Update questions
                    Console.WriteLine($"Removing {existing.Questions.Count} existing questions");
                    context.Questions.RemoveRange(existing.Questions);
                    context.SaveChanges(); // Save the deletion first
                    Console.WriteLine("Existing questions removed");
                    
                    int addedCount = 0;
                    foreach (var question in quiz.Questions)
                    {
                        // Validate question has text before adding
                        if (string.IsNullOrWhiteSpace(question.QuestionText))
                        {
                            Console.WriteLine($"Skipping empty question");
                            continue;
                        }
                        
                        // Create a new Question entity to avoid tracking conflicts
                        var newQuestion = new Question
                        {
                            Id = 0, // New question
                            QuizId = existing.Id,
                            QuestionText = question.QuestionText,
                            Type = question.Type,
                            CorrectAnswerIndex = question.CorrectAnswerIndex,
                            CorrectAnswer = question.CorrectAnswer,
                            TimeLimit = question.TimeLimit,
                            OptionsJson = question.OptionsJson // Use the JSON directly to avoid deserialization issues
                        };
                        
                        // Ensure Options are synced to JSON if not already set
                        if (string.IsNullOrEmpty(newQuestion.OptionsJson) || newQuestion.OptionsJson == "[]")
                        {
                            question.SyncOptionsToJson();
                            newQuestion.OptionsJson = question.OptionsJson;
                        }
                        
                        Console.WriteLine($"Adding question: '{newQuestion.QuestionText}' (Type: {newQuestion.Type}, QuizId: {newQuestion.QuizId}, OptionsJson: {newQuestion.OptionsJson})");
                        context.Questions.Add(newQuestion);
                        addedCount++;
                    }
                    
                    Console.WriteLine($"Added {addedCount} questions to context. Saving changes...");
                    context.SaveChanges();
                    Console.WriteLine("SaveChanges completed");
                    
                    // Force reload from database to ensure we have the latest data
                    context.Entry(existing).Collection(q => q.Questions).Load();
                    Console.WriteLine($"After reload: Quiz has {existing.Questions.Count} questions");
                    
                    // Verify questions were saved
                    var verifyQuestions = context.Questions.Where(q => q.QuizId == existing.Id).ToList();
                    Console.WriteLine($"Verification: Found {verifyQuestions.Count} questions in database for quiz {existing.Id}");
                }
                else
                {
                    // Quiz doesn't exist, add as new
                    // Set the UserId to the current logged-in user
                    quiz.UserId = _userSession.CurrentUserId ?? 0;
                    
                    // Create new Question entities to avoid tracking conflicts
                    var newQuestions = new List<Question>();
                    foreach (var question in quiz.Questions)
                    {
                        if (string.IsNullOrWhiteSpace(question.QuestionText))
                        {
                            continue;
                        }
                        
                        question.SyncOptionsToJson();
                        var newQuestion = new Question
                        {
                            Id = 0,
                            QuizId = 0, // Will be set when quiz is saved
                            QuestionText = question.QuestionText,
                            Type = question.Type,
                            CorrectAnswerIndex = question.CorrectAnswerIndex,
                            CorrectAnswer = question.CorrectAnswer,
                            TimeLimit = question.TimeLimit,
                            OptionsJson = question.OptionsJson
                        };
                        newQuestions.Add(newQuestion);
                    }
                    
                    quiz.Questions = newQuestions;
                    context.Quizzes.Add(quiz);
                    context.SaveChanges();
                    
                    // Set QuizId for questions after quiz is saved
                    foreach (var question in quiz.Questions)
                    {
                        question.QuizId = quiz.Id;
                    }
                    
                    if (quiz.Questions.Count > 0)
                    {
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating quiz {quiz.Id}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Gets all published quizzes from all teachers (for students to view)
        /// </summary>
        public List<Quiz> GetAllPublishedQuizzesForStudents()
        {
            try
            {
                // Use shared quiz database
                using var quizContext = _dbContextFactory.GetQuizDbContext();
                var quizzes = quizContext.Quizzes
                    .Include(q => q.Questions)
                    .Where(q => q.IsPublished && !q.IsPrivate) // Only show public quizzes
                    .ToList();
                
                Console.WriteLine($"GetAllPublishedQuizzesForStudents: Found {quizzes.Count} published public quizzes");
                
                // Create new objects to avoid tracking issues
                var result = new List<Quiz>();
                foreach (var quiz in quizzes)
                {
                    var newQuiz = new Quiz
                    {
                        Id = quiz.Id,
                        Title = quiz.Title,
                        Description = quiz.Description,
                        QuizCode = quiz.QuizCode,
                        IsPrivate = quiz.IsPrivate,
                        TotalTimeLimit = quiz.TotalTimeLimit,
                        IsPublished = quiz.IsPublished,
                        UserId = quiz.UserId,
                        Questions = new List<Question>()
                    };
                    
                    if (quiz.Questions != null && quiz.Questions.Count > 0)
                    {
                        foreach (var question in quiz.Questions)
                        {
                            var options = question.Options;
                            var newQuestion = new Question
                            {
                                Id = question.Id,
                                QuizId = question.QuizId,
                                QuestionText = question.QuestionText,
                                Type = question.Type,
                                CorrectAnswerIndex = question.CorrectAnswerIndex,
                                CorrectAnswer = question.CorrectAnswer,
                                TimeLimit = question.TimeLimit,
                                OptionsJson = question.OptionsJson,
                                HasQuestionTimer = question.TimeLimit.HasValue
                            };

                            if (options != null && options.Count > 0)
                            {
                                newQuestion.Options = options;
                            }

                            newQuiz.Questions.Add(newQuestion);
                        }
                    }

                    result.Add(newQuiz);
                }
                
                Console.WriteLine($"GetAllPublishedQuizzesForStudents: Returning {result.Count} total published quizzes");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllPublishedQuizzesForStudents: Error: {ex.Message}");
                return new List<Quiz>();
            }
        }

        public List<Quiz> GetPublishedQuizzes()
        {
            try
            {
                using var context = GetUserContext();
                var currentUserId = _userSession.CurrentUserId ?? 0;
                
                var quizzes = context.Quizzes
                    .Include(q => q.Questions)
                    .Where(q => q.IsPublished && q.UserId == currentUserId)
                    .ToList();
                
                // Create new objects to avoid tracking issues
                var result = new List<Quiz>();
                foreach (var quiz in quizzes)
                {
                    var newQuiz = new Quiz
                    {
                        Id = quiz.Id,
                        Title = quiz.Title,
                        Description = quiz.Description,
                        QuizCode = quiz.QuizCode,
                        IsPrivate = quiz.IsPrivate,
                        TotalTimeLimit = quiz.TotalTimeLimit,
                        IsPublished = quiz.IsPublished,
                        UserId = quiz.UserId,
                        Questions = new List<Question>()
                    };
                    
                    if (quiz.Questions != null && quiz.Questions.Count > 0)
                    {
                        foreach (var question in quiz.Questions)
                        {
                            var options = question.Options;
                            var newQuestion = new Question
                            {
                                Id = question.Id,
                                QuizId = question.QuizId,
                                QuestionText = question.QuestionText,
                                Type = question.Type,
                                CorrectAnswerIndex = question.CorrectAnswerIndex,
                                CorrectAnswer = question.CorrectAnswer,
                                TimeLimit = question.TimeLimit,
                                OptionsJson = question.OptionsJson,
                                HasQuestionTimer = question.TimeLimit.HasValue
                            };
                            
                            if (options != null && options.Count > 0)
                            {
                                newQuestion.Options = options;
                            }
                            
                            newQuiz.Questions.Add(newQuestion);
                        }
                    }
                    
                    result.Add(newQuiz);
                }
                
                Console.WriteLine($"GetPublishedQuizzes: Returning {result.Count} quizzes with total {result.Sum(q => q.Questions?.Count ?? 0)} questions");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting published quizzes: {ex.Message}");
                    return new List<Quiz>();
            }
        }

        public List<Quiz> GetUnpublishedQuizzes()
        {
            try
            {
                using var context = GetUserContext();
                var currentUserId = _userSession.CurrentUserId ?? 0;
                
                var quizzes = context.Quizzes
                    .Include(q => q.Questions)
                    .Where(q => !q.IsPublished && q.UserId == currentUserId)
                    .ToList();
                
                // Create new objects to avoid tracking issues
                var result = new List<Quiz>();
                foreach (var quiz in quizzes)
                {
                    var newQuiz = new Quiz
                    {
                        Id = quiz.Id,
                        Title = quiz.Title,
                        Description = quiz.Description,
                        QuizCode = quiz.QuizCode,
                        IsPrivate = quiz.IsPrivate,
                        TotalTimeLimit = quiz.TotalTimeLimit,
                        IsPublished = quiz.IsPublished,
                        UserId = quiz.UserId,
                        Questions = new List<Question>()
                    };
                    
                    if (quiz.Questions != null && quiz.Questions.Count > 0)
                    {
                        foreach (var question in quiz.Questions)
                        {
                            var options = question.Options;
                            var newQuestion = new Question
                            {
                                Id = question.Id,
                                QuizId = question.QuizId,
                                QuestionText = question.QuestionText,
                                Type = question.Type,
                                CorrectAnswerIndex = question.CorrectAnswerIndex,
                                CorrectAnswer = question.CorrectAnswer,
                                TimeLimit = question.TimeLimit,
                                OptionsJson = question.OptionsJson,
                                HasQuestionTimer = question.TimeLimit.HasValue
                            };
                            
                            if (options != null && options.Count > 0)
                            {
                                newQuestion.Options = options;
                            }
                            
                            newQuiz.Questions.Add(newQuestion);
                        }
                    }
                    
                    result.Add(newQuiz);
                }
                
                Console.WriteLine($"GetUnpublishedQuizzes: Returning {result.Count} quizzes with total {result.Sum(q => q.Questions?.Count ?? 0)} questions");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting unpublished quizzes: {ex.Message}");
                return new List<Quiz>();
            }
        }

        /// <summary>
        /// Gets a quiz by ID from all teacher databases (for students)
        /// This searches both public and private published quizzes
        /// </summary>
        public Quiz? GetQuizByIdForStudent(int quizId)
        {
            try
            {
                // Use shared quiz database
                using var quizContext = _dbContextFactory.GetQuizDbContext();
                var quiz = quizContext.Quizzes
                    .Include(q => q.Questions)
                    .Where(q => q.Id == quizId && q.IsPublished) // Must be published
                    .FirstOrDefault();

                if (quiz != null)
                {
                    Console.WriteLine($"GetQuizByIdForStudent: Found quiz '{quiz.Title}' (ID: {quiz.Id})");

                            // Create new objects to avoid tracking issues
                            var result = new Quiz
                            {
                                Id = quiz.Id,
                                Title = quiz.Title,
                                Description = quiz.Description,
                                QuizCode = quiz.QuizCode,
                                IsPrivate = quiz.IsPrivate,
                                TotalTimeLimit = quiz.TotalTimeLimit,
                                IsPublished = quiz.IsPublished,
                                UserId = quiz.UserId,
                                Questions = new List<Question>()
                            };

                            if (quiz.Questions != null && quiz.Questions.Count > 0)
                            {
                                foreach (var question in quiz.Questions)
                                {
                                    var options = question.Options;
                                    var newQuestion = new Question
                                    {
                                        Id = question.Id,
                                        QuizId = question.QuizId,
                                        QuestionText = question.QuestionText,
                                        Type = question.Type,
                                        CorrectAnswerIndex = question.CorrectAnswerIndex,
                                        CorrectAnswer = question.CorrectAnswer,
                                        TimeLimit = question.TimeLimit,
                                        OptionsJson = question.OptionsJson,
                                        HasQuestionTimer = question.TimeLimit.HasValue
                                    };
                                    
                                    if (options != null && options.Count > 0)
                                    {
                                        newQuestion.Options = options;
                                    }
                                    
                                    result.Questions.Add(newQuestion);
                                }
                            }

                    return result;
                }

                Console.WriteLine($"GetQuizByIdForStudent: Quiz {quizId} not found");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting quiz by id {quizId} for student: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets a quiz by code from all teacher databases (for students to access private quizzes)
        /// </summary>
        public Quiz? GetQuizByCodeForStudent(string quizCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(quizCode))
                {
                    return null;
                }
                
                var normalizedCode = quizCode.Trim().ToUpper();

                // Use shared quiz database
                using var quizContext = _dbContextFactory.GetQuizDbContext();
                var quiz = quizContext.Quizzes
                    .Include(q => q.Questions)
                    .Where(q => q.IsPublished && q.IsPrivate && q.QuizCode == normalizedCode)
                    .FirstOrDefault();

                if (quiz != null)
                {
                    Console.WriteLine($"GetQuizByCodeForStudent: Found quiz '{quiz.Title}' (ID: {quiz.Id}) with code '{normalizedCode}'");

                            // Create new objects to avoid tracking issues
                            var result = new Quiz
                            {
                                Id = quiz.Id,
                                Title = quiz.Title,
                                Description = quiz.Description,
                                QuizCode = quiz.QuizCode,
                                IsPrivate = quiz.IsPrivate,
                                TotalTimeLimit = quiz.TotalTimeLimit,
                                IsPublished = quiz.IsPublished,
                                UserId = quiz.UserId,
                                Questions = new List<Question>()
                            };

                            if (quiz.Questions != null && quiz.Questions.Count > 0)
                    {
                        foreach (var question in quiz.Questions)
                        {
                                    var options = question.Options;
                                    var newQuestion = new Question
                                    {
                                        Id = question.Id,
                                        QuizId = question.QuizId,
                                        QuestionText = question.QuestionText,
                                        Type = question.Type,
                                        CorrectAnswerIndex = question.CorrectAnswerIndex,
                                        CorrectAnswer = question.CorrectAnswer,
                                        TimeLimit = question.TimeLimit,
                                        OptionsJson = question.OptionsJson,
                                        HasQuestionTimer = question.TimeLimit.HasValue
                                    };

                                    if (options != null && options.Count > 0)
                                    {
                                        newQuestion.Options = options;
                                    }

                                    result.Questions.Add(newQuestion);
                                }
                            }

                    return result;
                }

                Console.WriteLine($"GetQuizByCodeForStudent: No quiz found with code '{normalizedCode}'");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetQuizByCodeForStudent: Error: {ex.Message}");
                return null;
            }
        }

        public Quiz? GetQuizById(int id)
        {
            try
            {
                using var context = GetUserContext();
                var currentUserId = _userSession.CurrentUserId ?? 0;
                
                // Filter by UserId to ensure users only see their own quizzes
                var quiz = context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefault(q => q.Id == id && q.UserId == currentUserId);
                
                if (quiz == null)
                {
                    return null;
                }
                
                // Force load questions and ensure Options are deserialized
                // Create a new Quiz object to avoid tracking issues when context is disposed
                var result = new Quiz
                {
                    Id = quiz.Id,
                    Title = quiz.Title,
                    Description = quiz.Description,
                    QuizCode = quiz.QuizCode,
                    IsPrivate = quiz.IsPrivate,
                    TotalTimeLimit = quiz.TotalTimeLimit,
                    IsPublished = quiz.IsPublished,
                    UserId = quiz.UserId,
                    Questions = new List<Question>()
                };
                
                // Copy questions to avoid tracking issues
                if (quiz.Questions != null && quiz.Questions.Count > 0)
                {
                    Console.WriteLine($"GetQuizById: Found {quiz.Questions.Count} questions in database for quiz {id}");
                    foreach (var question in quiz.Questions)
                    {
                        // Access Options property to trigger deserialization
                        var options = question.Options;
                        Console.WriteLine($"GetQuizById: Question {question.Id}: '{question.QuestionText}', Type: {question.Type}, OptionsJson: {question.OptionsJson}, Options count: {options?.Count ?? 0}");
                        
                        var newQuestion = new Question
                        {
                            Id = question.Id,
                            QuizId = question.QuizId,
                            QuestionText = question.QuestionText,
                            Type = question.Type,
                            CorrectAnswerIndex = question.CorrectAnswerIndex,
                            CorrectAnswer = question.CorrectAnswer,
                            TimeLimit = question.TimeLimit,
                            OptionsJson = question.OptionsJson,
                            HasQuestionTimer = question.TimeLimit.HasValue
                        };
                        
                        // Explicitly set Options to ensure it's deserialized
                        if (options != null && options.Count > 0)
                        {
                            newQuestion.Options = options;
                        }
                        
                        result.Questions.Add(newQuestion);
                    }
                }
                else
                {
                    Console.WriteLine($"GetQuizById: No questions found in database for quiz {id}");
                }
                
                Console.WriteLine($"GetQuizById: Returning quiz {id} with {result.Questions.Count} questions");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting quiz by id {id}: {ex.Message}");
                return null;
            }
        }

        public void DeleteQuiz(int id)
        {
            try
            {
                using var context = GetUserContext();
                var currentUserId = _userSession.CurrentUserId ?? 0;
                
                // Filter by UserId to ensure users can only delete their own quizzes
                var quiz = context.Quizzes
                    .Include(q => q.Questions)
                    .FirstOrDefault(q => q.Id == id && q.UserId == currentUserId);
                
                if (quiz != null)
                {
                    context.Quizzes.Remove(quiz);
                    context.SaveChanges();
                }
                else
                {
                    throw new UnauthorizedAccessException("Quiz not found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting quiz {id}: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Gets all quizzes for the current user (both published and unpublished)
        /// </summary>
        public List<Quiz> GetAllQuizzes()
        {
            try
            {
                using var context = GetUserContext();
                var currentUserId = _userSession.CurrentUserId ?? 0;
                
                // First, get all quizzes to see what's in the database
                var allQuizzes = context.Quizzes.ToList();
                Console.WriteLine($"GetAllQuizzes: Total quizzes in database: {allQuizzes.Count}");
                foreach (var q in allQuizzes)
                {
                    Console.WriteLine($"  - Quiz ID: {q.Id}, Title: {q.Title}, UserId: {q.UserId}");
                }
                
                var quizzes = context.Quizzes
                    .Include(q => q.Questions)
                    .Where(q => q.UserId == currentUserId)
                    .OrderByDescending(q => q.Id)
                    .ToList();
                
                Console.WriteLine($"GetAllQuizzes: Found {quizzes.Count} quizzes for UserId={currentUserId}");
                
                // Create new objects to avoid tracking issues
                var result = new List<Quiz>();
                foreach (var quiz in quizzes)
                {
                    var newQuiz = new Quiz
                    {
                        Id = quiz.Id,
                        Title = quiz.Title,
                        Description = quiz.Description,
                        QuizCode = quiz.QuizCode,
                        IsPrivate = quiz.IsPrivate,
                        TotalTimeLimit = quiz.TotalTimeLimit,
                        IsPublished = quiz.IsPublished,
                        UserId = quiz.UserId,
                        Questions = new List<Question>()
                    };
                    
                    if (quiz.Questions != null && quiz.Questions.Count > 0)
                    {
                        foreach (var question in quiz.Questions)
                        {
                            var options = question.Options;
                            var newQuestion = new Question
                            {
                                Id = question.Id,
                                QuizId = question.QuizId,
                                QuestionText = question.QuestionText,
                                Type = question.Type,
                                CorrectAnswerIndex = question.CorrectAnswerIndex,
                                CorrectAnswer = question.CorrectAnswer,
                                TimeLimit = question.TimeLimit,
                                OptionsJson = question.OptionsJson,
                                HasQuestionTimer = question.TimeLimit.HasValue
                            };
                            
                            if (options != null && options.Count > 0)
                            {
                                newQuestion.Options = options;
                            }
                            
                            newQuiz.Questions.Add(newQuestion);
                        }
                    }
                    
                    result.Add(newQuiz);
                }
                
                Console.WriteLine($"GetAllQuizzes: Returning {result.Count} quizzes with total {result.Sum(q => q.Questions?.Count ?? 0)} questions");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting all quizzes: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<Quiz>();
            }
        }
    }
}
