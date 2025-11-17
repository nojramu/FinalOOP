using SmartQuiz.Components;
using SmartQuiz.Services;
using SmartQuiz.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add Razor + Blazor support
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ✅ Enable detailed errors for Blazor
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
});

// ✅ Add HttpContextAccessor and session support for session management
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ Register database connection (Users database for authentication)
var usersConnectionString = builder.Configuration.GetConnectionString("Users");
if (!string.IsNullOrEmpty(usersConnectionString) && usersConnectionString.Contains("Data Source="))
{
    var dbPath = usersConnectionString.Replace("Data Source=", "").Trim();
    // If relative path, make it absolute based on app base directory
    if (!System.IO.Path.IsPathRooted(dbPath))
    {
        // Try multiple locations: project root, app base directory, current directory
        var appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
        var projectRoot = System.IO.Directory.GetParent(appBaseDir)?.Parent?.Parent?.FullName ?? appBaseDir;
        var currentDir = System.IO.Directory.GetCurrentDirectory();
        
        // Check which directory has the database file or use project root
        var possiblePaths = new[]
        {
            System.IO.Path.Combine(projectRoot, dbPath),
            System.IO.Path.Combine(appBaseDir, dbPath),
            System.IO.Path.Combine(currentDir, dbPath),
            dbPath // Try as-is if it's in current directory
        };
        
        // Use the first path that exists, or project root if none exist
        dbPath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p)) ?? possiblePaths[0];
        
        // Ensure directory exists
        var dbDir = System.IO.Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDir) && !System.IO.Directory.Exists(dbDir))
        {
            System.IO.Directory.CreateDirectory(dbDir);
        }
        
        usersConnectionString = $"Data Source={dbPath}";
        Console.WriteLine($"Resolved Users database path: {dbPath}");
        Console.WriteLine($"Users database file exists: {System.IO.File.Exists(dbPath)}");
    }
}
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(usersConnectionString));

// ✅ Add your services
builder.Services.AddScoped<DbContextFactory>();
builder.Services.AddScoped<QuizService>();
builder.Services.AddScoped<QuizAttemptService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<UserSessionService>(); // Scoped to maintain session per request/circuit

var app = builder.Build();

// ✅ Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection(); // 👈 HTTPS only used in production
}

app.UseStaticFiles();
app.UseAntiforgery();
app.UseSession(); // Enable session middleware

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ✅ Ensure database is created and migrate schema
using (var scope = app.Services.CreateScope())
{
    try
    {
        // Initialize Users database
        var usersContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var usersConnection = usersContext.Database.GetDbConnection();
        var actualUsersConnectionString = usersConnection.ConnectionString;
        var usersDbPath = actualUsersConnectionString?.Replace("Data Source=", "").Trim() ?? "Unknown";
        Console.WriteLine($"Users database connection string: {actualUsersConnectionString}");
        Console.WriteLine($"Users database path: {usersDbPath}");
        Console.WriteLine($"Current working directory: {System.IO.Directory.GetCurrentDirectory()}");
        Console.WriteLine($"Users database file exists: {System.IO.File.Exists(usersDbPath)}");
        
        var wasOpen = usersConnection.State == System.Data.ConnectionState.Open;
        
        if (!wasOpen)
        {
            usersConnection.Open();
        }

        try
        {
            // Always ensure Users database and tables are created
            Console.WriteLine("Ensuring Users database and tables are created...");
            usersContext.Database.EnsureCreated();
            
            // Verify Users table exists
            using var checkCommand = usersConnection.CreateCommand();
            checkCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Users'";
            var usersResult = checkCommand.ExecuteScalar();
            
            if (usersResult == null)
            {
                Console.WriteLine("WARNING: Users table still does not exist after EnsureCreated. Attempting manual creation...");
                // Manually create Users table if EnsureCreated didn't work
                using var createCommand = usersConnection.CreateCommand();
                createCommand.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL DEFAULT '',
                        Email TEXT NOT NULL DEFAULT '',
                        Password TEXT NOT NULL DEFAULT '',
                        Role TEXT NOT NULL DEFAULT 'Student',
                        Bio TEXT NOT NULL DEFAULT '',
                        AlternativePassword TEXT NOT NULL DEFAULT ''
                    )";
                createCommand.ExecuteNonQuery();
                Console.WriteLine("Users table created manually.");
            }
            else
            {
                Console.WriteLine("Users table exists.");
            }
        }
        catch (Exception tableCheckEx)
        {
            Console.WriteLine($"Error checking/creating tables: {tableCheckEx.Message}");
            Console.WriteLine($"Stack trace: {tableCheckEx.StackTrace}");
            // If check fails, try manual creation
            try
            {
                using var createCommand = usersConnection.CreateCommand();
                createCommand.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL DEFAULT '',
                        Email TEXT NOT NULL DEFAULT '',
                        Password TEXT NOT NULL DEFAULT '',
                        Role TEXT NOT NULL DEFAULT 'Student',
                        Bio TEXT NOT NULL DEFAULT '',
                        AlternativePassword TEXT NOT NULL DEFAULT ''
                    )";
                createCommand.ExecuteNonQuery();
                Console.WriteLine("Users table created manually after error.");
            }
            catch (Exception manualCreateEx)
            {
                Console.WriteLine($"Error manually creating Users table: {manualCreateEx.Message}");
            }
        }

        // Check if Role column exists in Users table, if not add it
        try
        {
            using var command = usersConnection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Users') WHERE name='Role'";
            var result = command.ExecuteScalar();

            if (result != null && Convert.ToInt32(result) == 0)
            {
                // Role column doesn't exist, add it
                command.CommandText = "ALTER TABLE Users ADD COLUMN Role TEXT DEFAULT 'Student'";
                command.ExecuteNonQuery();
                Console.WriteLine("Added Role column to Users table.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not check/update Users table: {ex.Message}");
        }

        // Check if Bio column exists in Users table, if not add it
        try
        {
            using var command = usersConnection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Users') WHERE name='Bio'";
            var result = command.ExecuteScalar();

            if (result != null && Convert.ToInt32(result) == 0)
            {
                // Bio column doesn't exist, add it
                command.CommandText = "ALTER TABLE Users ADD COLUMN Bio TEXT DEFAULT ''";
                command.ExecuteNonQuery();
                Console.WriteLine("Added Bio column to Users table.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not check/update Users table for Bio column: {ex.Message}");
        }

        // Check if AlternativePassword column exists in Users table, if not add it
        try
        {
            using var command = usersConnection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Users') WHERE name='AlternativePassword'";
            var result = command.ExecuteScalar();

            if (result != null && Convert.ToInt32(result) == 0)
            {
                command.CommandText = "ALTER TABLE Users ADD COLUMN AlternativePassword TEXT NOT NULL DEFAULT ''";
                command.ExecuteNonQuery();
                Console.WriteLine("Added AlternativePassword column to Users table.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not check/update Users table for AlternativePassword column: {ex.Message}");
        }

        // Check if PhotoPath column exists in Users table, if not add it
        try
        {
            using var command = usersConnection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Users') WHERE name='PhotoPath'";
            var result = command.ExecuteScalar();

            if (result != null && Convert.ToInt32(result) == 0)
            {
                command.CommandText = "ALTER TABLE Users ADD COLUMN PhotoPath TEXT NOT NULL DEFAULT ''";
                command.ExecuteNonQuery();
                Console.WriteLine("Added PhotoPath column to Users table.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not check/update Users table for PhotoPath column: {ex.Message}");
        }

        // Ensure Notifications table exists in Users database
        try
        {
            using var checkNotificationsCommand = usersConnection.CreateCommand();
            checkNotificationsCommand.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Notifications'";
            var notificationsTableExists = checkNotificationsCommand.ExecuteScalar() != null;

            if (!notificationsTableExists)
            {
                Console.WriteLine("Creating Notifications table in Users database...");
                using var createNotificationsCommand = usersConnection.CreateCommand();
                createNotificationsCommand.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Notifications (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        StudentId INTEGER NOT NULL,
                        Type TEXT NOT NULL DEFAULT '',
                        Title TEXT NOT NULL DEFAULT '',
                        Message TEXT NOT NULL DEFAULT '',
                        QuizId INTEGER,
                        QuizAttemptId INTEGER,
                        Score REAL,
                        CreatedAt TEXT NOT NULL,
                        IsRead INTEGER NOT NULL DEFAULT 0
                    )";
                createNotificationsCommand.ExecuteNonQuery();
                Console.WriteLine("Notifications table created in Users database.");
            }
            else
            {
                Console.WriteLine("Notifications table already exists in Users database.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Could not check/create Notifications table: {ex.Message}");
        }

        // Initialize Quiz database (SmartQuiz_user_1.db)
        try
        {
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<DbContextFactory>();
            using var quizContext = dbContextFactory.GetQuizDbContext();
            var quizConnection = quizContext.Database.GetDbConnection();
            var quizConnectionString = quizConnection.ConnectionString;
            var quizDbPath = quizConnectionString?.Replace("Data Source=", "").Trim() ?? "Unknown";
            Console.WriteLine($"Quiz database connection string: {quizConnectionString}");
            Console.WriteLine($"Quiz database path: {quizDbPath}");
            Console.WriteLine($"Quiz database file exists: {System.IO.File.Exists(quizDbPath)}");
            
            var quizWasOpen = quizConnection.State == System.Data.ConnectionState.Open;
            if (!quizWasOpen)
            {
                quizConnection.Open();
            }

            try
            {
                // Ensure Quiz database and tables are created
                Console.WriteLine("Ensuring Quiz database and tables are created...");
                quizContext.Database.EnsureCreated();
                
                // Check if UserId column exists in Quizzes table, if not add it
                using var command = quizConnection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Quizzes') WHERE name='UserId'";
                var result = command.ExecuteScalar();

                if (result != null && Convert.ToInt32(result) == 0)
                {
                    // UserId column doesn't exist, add it
                    command.CommandText = "ALTER TABLE Quizzes ADD COLUMN UserId INTEGER NOT NULL DEFAULT 0";
                    command.ExecuteNonQuery();
                    Console.WriteLine("Added UserId column to Quizzes table.");
                }
                
                // Check if IsPrivate column exists in Quizzes table, if not add it
                command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Quizzes') WHERE name='IsPrivate'";
                result = command.ExecuteScalar();
                if (result != null && Convert.ToInt32(result) == 0)
                {
                    command.CommandText = "ALTER TABLE Quizzes ADD COLUMN IsPrivate INTEGER NOT NULL DEFAULT 0";
                    command.ExecuteNonQuery();
                    Console.WriteLine("Added IsPrivate column to Quizzes table.");
                }
            }
            finally
            {
                if (!quizWasOpen)
                {
                    quizConnection.Close();
                }
            }
        }
        catch (Exception quizEx)
        {
            Console.WriteLine($"Warning: Could not initialize Quiz database: {quizEx.Message}");
        }

        if (!wasOpen)
        {
            usersConnection.Close();
        }

        Console.WriteLine("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing database: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        
        // If initialization fails in development, try to recreate
        if (app.Environment.IsDevelopment())
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                Console.WriteLine("Attempting to recreate database...");
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                Console.WriteLine("Database recreated successfully.");
            }
            catch (Exception recreateEx)
            {
                Console.WriteLine($"Error recreating database: {recreateEx.Message}");
                Console.WriteLine($"Stack trace: {recreateEx.StackTrace}");
            }
        }
    }
}

app.Run();
