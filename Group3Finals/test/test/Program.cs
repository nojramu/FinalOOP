using test.Components;
using test.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SQLite database - use absolute path to ensure consistency
// This ensures the database file is always in the same location
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "app.db");
var connectionString = $"Data Source={dbPath}";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// Current user context (very simple state holder)
builder.Services.AddSingleton<test.Services.CurrentUserContext>();

// Ensure database is created
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Check if database file exists before ensuring creation
    // This prevents schema recreation that could delete data
    var dbExists = File.Exists(dbPath);

    if (!dbExists)
    {
        // Only create database if file doesn't exist
        dbContext.Database.EnsureCreated();
        Console.WriteLine($"Database created at: {dbPath}");
    }
    else
    {
        // If database exists, just ensure it can connect (don't recreate)
        try
        {
            dbContext.Database.CanConnect();
            Console.WriteLine($"Database found at: {dbPath}");
        }
        catch
        {
            // If connection fails, try to ensure created (might be corrupted)
            dbContext.Database.EnsureCreated();
        }
    }

    // Ensure Orders table exists (non-destructive)
    using (var conn = dbContext.Database.GetDbConnection())
    {
        conn.Open();
        using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name='Orders';";
        var exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

        if (!exists)
        {
            using var createCmd = conn.CreateCommand();
            createCmd.CommandText = @"
CREATE TABLE Orders (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Username TEXT NULL,
  CustomerName TEXT NOT NULL,
  HouseNo TEXT NOT NULL,
  Street TEXT NOT NULL,
  Barangay TEXT NOT NULL,
  City TEXT NULL,
  UseSavedAddress INTEGER NOT NULL,
  HasGallon INTEGER NOT NULL,
  PurchaseGallon INTEGER NOT NULL,
  RefillGallon INTEGER NOT NULL,
  Type TEXT NOT NULL,
  Quantity INTEGER NOT NULL,
  PaymentMethod TEXT NOT NULL,
  TotalPrice REAL NOT NULL,
  Status TEXT NULL,
  CreatedAt TEXT NOT NULL
);";
            createCmd.ExecuteNonQuery();
            Console.WriteLine("Orders table created.");
        }
        else
        {
            // If the table exists, ensure the Username column exists; add it if missing
            using var pragmaCmd = conn.CreateCommand();
            pragmaCmd.CommandText = "PRAGMA table_info(Orders);";
            var hasUsername = false;
            var hasTimeDelivered = false;
            using (var reader = pragmaCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var colName = reader.GetString(1);
                    if (string.Equals(colName, "Username", StringComparison.OrdinalIgnoreCase))
                    {
                        hasUsername = true;
                    }
                    if (string.Equals(colName, "TimeDelivered", StringComparison.OrdinalIgnoreCase))
                    {
                        hasTimeDelivered = true;
                    }
                }
            }

            if (hasUsername)
            {
                // nothing to do
            }
            else
            {
                using var alterCmd = conn.CreateCommand();
                alterCmd.CommandText = "ALTER TABLE Orders ADD COLUMN Username TEXT NULL;";
                alterCmd.ExecuteNonQuery();
                Console.WriteLine("Orders table migrated (added Username column).");
            }

            if (!hasTimeDelivered)
            {
                using var alter2 = conn.CreateCommand();
                alter2.CommandText = "ALTER TABLE Orders ADD COLUMN TimeDelivered TEXT NULL;";
                alter2.ExecuteNonQuery();
                Console.WriteLine("Orders table migrated (added TimeDelivered column).");
            }
        }

        // Ensure Feedbacks table exists (non-destructive)
        using (var conn2 = dbContext.Database.GetDbConnection())
        {
            conn2.Open();
            using var checkCmd2 = conn2.CreateCommand();
            checkCmd2.CommandText = "SELECT COUNT(1) FROM sqlite_master WHERE type='table' AND name='Feedbacks';";
            var exists2 = Convert.ToInt32(checkCmd2.ExecuteScalar()) > 0;

            if (!exists2)
            {
                using var createCmd2 = conn2.CreateCommand();
                createCmd2.CommandText = @"
CREATE TABLE Feedbacks (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Username TEXT NULL,
  FullName TEXT NOT NULL,
  IsAnonymous INTEGER NOT NULL,
  Rating INTEGER NOT NULL,
  Comment TEXT NOT NULL,
  AdminReply TEXT NULL,
  AdminReplyDate TEXT NULL,
  CreatedAt TEXT NOT NULL
);";
                createCmd2.ExecuteNonQuery();
                Console.WriteLine("Feedbacks table created.");
            }
            else
            {
                // If the table exists, ensure all required columns exist; add them if missing
                using var pragmaCmd2 = conn2.CreateCommand();
                pragmaCmd2.CommandText = "PRAGMA table_info(Feedbacks);";
                var cols2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                using (var reader2 = pragmaCmd2.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        cols2.Add(reader2.GetString(1));
                    }
                }

                // Remove DisplayName column if it exists (it's a computed property, not a database column)
                if (cols2.Contains("DisplayName"))
                {
                    // SQLite doesn't support DROP COLUMN directly, so we need to recreate the table
                    // But to preserve data, we'll create a new table, copy data, drop old, rename new
                    using var dropDisplayName = conn2.CreateCommand();
                    dropDisplayName.CommandText = @"
CREATE TABLE Feedbacks_new (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Username TEXT NULL,
  FullName TEXT NOT NULL,
  IsAnonymous INTEGER NOT NULL,
  Rating INTEGER NOT NULL,
  Comment TEXT NOT NULL,
  AdminReply TEXT NULL,
  AdminReplyDate TEXT NULL,
  CreatedAt TEXT NOT NULL
);
INSERT INTO Feedbacks_new (Id, Username, FullName, IsAnonymous, Rating, Comment, AdminReply, AdminReplyDate, CreatedAt)
SELECT Id, Username, FullName, IsAnonymous, Rating, Comment, AdminReply, AdminReplyDate, CreatedAt FROM Feedbacks;
DROP TABLE Feedbacks;
ALTER TABLE Feedbacks_new RENAME TO Feedbacks;
";
                    dropDisplayName.ExecuteNonQuery();
                    Console.WriteLine("Feedbacks table migrated (removed DisplayName column).");
                    // Re-read columns after migration
                    cols2.Clear();
                    pragmaCmd2.CommandText = "PRAGMA table_info(Feedbacks);";
                    using (var reader2 = pragmaCmd2.ExecuteReader())
                    {
                        while (reader2.Read())
                        {
                            cols2.Add(reader2.GetString(1));
                        }
                    }
                }

                void EnsureColumn2(string name, string ddl)
                {
                    if (!cols2.Contains(name))
                    {
                        using var alter2 = conn2.CreateCommand();
                        alter2.CommandText = $"ALTER TABLE Feedbacks ADD COLUMN {ddl};";
                        alter2.ExecuteNonQuery();
                        Console.WriteLine($"Feedbacks table migrated (added {name} column).");
                    }
                }

                EnsureColumn2("Username", "Username TEXT NULL");
                EnsureColumn2("FullName", "FullName TEXT NOT NULL DEFAULT ''");
                EnsureColumn2("IsAnonymous", "IsAnonymous INTEGER NOT NULL DEFAULT 0");
                EnsureColumn2("Rating", "Rating INTEGER NOT NULL DEFAULT 0");
                EnsureColumn2("Comment", "Comment TEXT NOT NULL DEFAULT ''");
                EnsureColumn2("AdminReply", "AdminReply TEXT NULL");
                EnsureColumn2("AdminReplyDate", "AdminReplyDate TEXT NULL");
                EnsureColumn2("CreatedAt", "CreatedAt TEXT NOT NULL DEFAULT ''");
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
