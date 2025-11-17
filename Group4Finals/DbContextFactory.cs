using System.Linq;
using Microsoft.EntityFrameworkCore;
using SmartQuiz.Data;
using SmartQuiz.Services;

namespace SmartQuiz.Services
{
    public class DbContextFactory
    {
        private readonly IConfiguration _configuration;
        private readonly UserSessionService _userSession;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DbContextFactory(
            IConfiguration configuration,
            UserSessionService userSession,
            IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _userSession = userSession;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the Users database context (for Users table)
        /// </summary>
        public AppDbContext GetMasterDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = _configuration.GetConnectionString("Users");
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = _configuration.GetConnectionString("Default"); // Fallback
            }
            optionsBuilder.UseSqlite(connectionString);
            return new AppDbContext(optionsBuilder.Options);
        }

        /// <summary>
        /// Gets the Quiz database context (for Quizzes, Questions, QuizAttempts, StudentAnswers)
        /// </summary>
        public AppDbContext GetQuizDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = _configuration.GetConnectionString("QuizData");
            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback to SmartQuiz_user_1.db
                var appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
                var projectRoot = System.IO.Directory.GetParent(appBaseDir)?.Parent?.Parent?.FullName ?? appBaseDir;
                var dbPath = System.IO.Path.Combine(projectRoot, "SmartQuiz_user_1.db");
                connectionString = $"Data Source={dbPath}";
            }
            
            // Resolve relative path
            if (connectionString.Contains("Data Source="))
            {
                var dbPath = connectionString.Replace("Data Source=", "").Trim();
                if (!System.IO.Path.IsPathRooted(dbPath))
                {
                    var appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
                    var projectRoot = System.IO.Directory.GetParent(appBaseDir)?.Parent?.Parent?.FullName ?? appBaseDir;
                    var currentDir = System.IO.Directory.GetCurrentDirectory();
                    
                    var possiblePaths = new[]
                    {
                        System.IO.Path.Combine(projectRoot, dbPath),
                        System.IO.Path.Combine(appBaseDir, dbPath),
                        System.IO.Path.Combine(currentDir, dbPath),
                        dbPath
                    };
                    
                    dbPath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? possiblePaths[0];
                    
                    // Ensure directory exists
                    var dbDir = System.IO.Path.GetDirectoryName(dbPath);
                    if (!string.IsNullOrEmpty(dbDir) && !System.IO.Directory.Exists(dbDir))
                    {
                        System.IO.Directory.CreateDirectory(dbDir);
                    }
                    
                    connectionString = $"Data Source={dbPath}";
                }
            }
            
            optionsBuilder.UseSqlite(connectionString);
            return new AppDbContext(optionsBuilder.Options);
        }

        /// <summary>
        /// Gets the user-specific database context (for Quizzes and Questions)
        /// Now uses the shared QuizData database (SmartQuiz_user_1.db)
        /// </summary>
        public AppDbContext GetUserDbContext()
        {
            // Use the shared QuizData database instead of per-user databases
            return GetQuizDbContext();
        }

        /// <summary>
        /// Gets the user-specific database context for a specific user ID
        /// </summary>
        public AppDbContext GetUserDbContext(int userId)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = GetUserConnectionString(userId);
            Console.WriteLine($"GetUserDbContext: Creating context for UserId={userId}, ConnectionString={connectionString}");
            optionsBuilder.UseSqlite(connectionString);
            var context = new AppDbContext(optionsBuilder.Options);
            
            // Ensure database exists
            try
            {
                context.Database.EnsureCreated();
                
                // Add IsPrivate column if it doesn't exist (for existing databases)
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
                        // Check if IsPrivate column exists in Quizzes table
                        using var checkCommand = connection.CreateCommand();
                        checkCommand.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Quizzes') WHERE name='IsPrivate'";
                        var result = checkCommand.ExecuteScalar();

                        if (result != null && Convert.ToInt32(result) == 0)
                        {
                            // Column doesn't exist, add it
                            Console.WriteLine($"Adding IsPrivate column to Quizzes table in user database {userId}...");
                            using var alterCommand = connection.CreateCommand();
                            alterCommand.CommandText = "ALTER TABLE Quizzes ADD COLUMN IsPrivate INTEGER NOT NULL DEFAULT 0";
                            alterCommand.ExecuteNonQuery();
                            Console.WriteLine($"IsPrivate column added successfully to user database {userId}");
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
                catch (Exception colEx)
                {
                    Console.WriteLine($"Error checking/adding IsPrivate column for user {userId}: {colEx.Message}");
                    // Don't throw - this is just for backward compatibility
                }
                
                // Verify database file was created
                var dbPath = GetUserConnectionString(userId).Replace("Data Source=", "").Trim();
                if (System.IO.File.Exists(dbPath))
                {
                    var fileInfo = new System.IO.FileInfo(dbPath);
                    Console.WriteLine($"GetUserDbContext: Database file exists for UserId={userId}, Size={fileInfo.Length} bytes, Path={dbPath}");
                }
                else
                {
                    Console.WriteLine($"GetUserDbContext: WARNING - Database file NOT found at {dbPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserDbContext: Error ensuring database for UserId={userId}: {ex.Message}");
                Console.WriteLine($"GetUserDbContext: Stack trace: {ex.StackTrace}");
                throw;
            }
            
            return context;
        }

        /// <summary>
        /// Gets the connection string for a user's database
        /// </summary>
        public string GetUserConnectionString(int userId)
        {
            // Get the directory where the master database is located
            var masterConnectionString = _configuration.GetConnectionString("Default");
            string dbPath;
            
            if (masterConnectionString != null && masterConnectionString.Contains("Data Source="))
            {
                var masterPath = masterConnectionString.Replace("Data Source=", "").Trim();
                
                if (System.IO.Path.IsPathRooted(masterPath))
                {
                    // Master database uses absolute path, use same directory
                    var masterDir = System.IO.Path.GetDirectoryName(masterPath);
                    dbPath = System.IO.Path.Combine(masterDir ?? "", $"SmartQuiz_user_{userId}.db");
                }
                else
                {
                    // Master database uses relative path, resolve it
                    var masterDir = System.IO.Path.GetDirectoryName(masterPath);
                    if (string.IsNullOrEmpty(masterDir))
                    {
                        // Master DB is in current directory, find where it actually is
                        var appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
                        var projectRoot = System.IO.Directory.GetParent(appBaseDir)?.Parent?.Parent?.FullName ?? appBaseDir;
                        
                        // Check if SmartQuiz.db exists in project root
                        var masterDbPath = System.IO.Path.Combine(projectRoot, "SmartQuiz.db");
                        if (System.IO.File.Exists(masterDbPath))
                        {
                            dbPath = System.IO.Path.Combine(projectRoot, $"SmartQuiz_user_{userId}.db");
                        }
                        else
                        {
                            // Use app base directory
                            dbPath = System.IO.Path.Combine(appBaseDir, $"SmartQuiz_user_{userId}.db");
                        }
                    }
                    else
                    {
                        dbPath = System.IO.Path.Combine(masterDir, $"SmartQuiz_user_{userId}.db");
                    }
                }
            }
            else
            {
                // Fallback: use project root
                var appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
                var projectRoot = System.IO.Directory.GetParent(appBaseDir)?.Parent?.Parent?.FullName ?? appBaseDir;
                dbPath = System.IO.Path.Combine(projectRoot, $"SmartQuiz_user_{userId}.db");
            }
            
            Console.WriteLine($"GetUserConnectionString: UserId={userId}, DatabasePath={dbPath}");
            return $"Data Source={dbPath}";
        }

        /// <summary>
        /// Gets the database file path for a user
        /// </summary>
        public string GetUserDatabasePath(int userId)
        {
            return $"SmartQuiz_user_{userId}.db";
        }

        /// <summary>
        /// Ensures the user database exists and is created
        /// </summary>
        public void EnsureUserDatabaseExists(int userId)
        {
            using var context = GetUserDbContext(userId);
            context.Database.EnsureCreated();
            
            // Add IsPrivate column if it doesn't exist (for existing databases)
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
                    // Check if IsPrivate column exists in Quizzes table
                    using var checkCommand = connection.CreateCommand();
                    checkCommand.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Quizzes') WHERE name='IsPrivate'";
                    var result = checkCommand.ExecuteScalar();

                    if (result != null && Convert.ToInt32(result) == 0)
                    {
                        // Column doesn't exist, add it
                        Console.WriteLine($"Adding IsPrivate column to Quizzes table in user database {userId}...");
                        using var alterCommand = connection.CreateCommand();
                        alterCommand.CommandText = "ALTER TABLE Quizzes ADD COLUMN IsPrivate INTEGER NOT NULL DEFAULT 0";
                        alterCommand.ExecuteNonQuery();
                        Console.WriteLine($"IsPrivate column added successfully to user database {userId}");
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
                Console.WriteLine($"Error checking/adding IsPrivate column for user {userId}: {ex.Message}");
                // Don't throw - this is just for backward compatibility
            }
        }
    }
}

