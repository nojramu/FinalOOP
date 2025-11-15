using Group5.Components;
using Group5.Services;
using Group5.Data;
using Group5.Shared;
using Group5.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<Group5.Services.Interfaces.IEmailService, Group5.Services.EmailService>();
builder.Services.AddScoped<Group5.Services.Interfaces.IInventoryService, Group5.Services.InventoryService>();

builder.Services.AddScoped(typeof(Group5.Data.Repositories.IRepository<>), typeof(Group5.Data.Repositories.Repository<>));
builder.Services.AddScoped<Group5.Data.Repositories.IInventoryRepository, Group5.Data.Repositories.InventoryRepository>();

builder.Services.AddScoped<Group5.Services.Business.CartManager>();
builder.Services.AddScoped<Group5.Services.Business.BorrowManager>();

builder.Services.AddHttpContextAccessor();

// Register EF Core DbContext (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}


app.UseAntiforgery();

app.UseStaticFiles();

// Initialize UserSession with ServiceProvider
UserSession.Initialize(app.Services);

// Ensure database is created with all tables
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Ensure all tables exist (will create missing tables without deleting existing ones)
    try
    {
        dbContext.Database.EnsureCreated();
        
        // Verify and create missing tables manually if needed
        var connection = dbContext.Database.GetDbConnection();
        connection.Open();
        
        using (var command = connection.CreateCommand())
        {
            // Add Name and StudentNumber columns to Users table if they don't exist
            try
            {
                // Check if Name column exists, if not add it
                command.CommandText = "PRAGMA table_info(Users)";
                var reader = command.ExecuteReader();
                var columns = new List<string>();
                while (reader.Read())
                {
                    columns.Add(reader.GetString(1)); // Column name is at index 1
                }
                reader.Close();
                
                if (!columns.Contains("Name"))
                {
                    command.CommandText = "ALTER TABLE Users ADD COLUMN Name TEXT NOT NULL DEFAULT ''";
                    command.ExecuteNonQuery();
                }
                
                if (!columns.Contains("StudentNumber"))
                {
                    command.CommandText = "ALTER TABLE Users ADD COLUMN StudentNumber TEXT NOT NULL DEFAULT ''";
                    command.ExecuteNonQuery();
                }
                
                // Add account lockout columns if they don't exist
                if (!columns.Contains("FailedLoginAttempts"))
                {
                    command.CommandText = "ALTER TABLE Users ADD COLUMN FailedLoginAttempts INTEGER NOT NULL DEFAULT 0";
                    command.ExecuteNonQuery();
                }
                
                if (!columns.Contains("LockedUntil"))
                {
                    command.CommandText = "ALTER TABLE Users ADD COLUMN LockedUntil TEXT";
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error adding columns to Users table: {ex.Message}");
            }
            
            // Check and create CartItems table if it doesn't exist
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS CartItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Department TEXT NOT NULL,
                    ItemName TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    UserEmail TEXT NOT NULL
                )";
            command.ExecuteNonQuery();
            
            // Check and create OfficialBorrowListRecords table if it doesn't exist
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS OfficialBorrowListRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentName TEXT NOT NULL,
                    StudentNumber TEXT NOT NULL,
                    ReferenceCode TEXT NOT NULL,
                    ProfessorInCharge TEXT NOT NULL,
                    BorrowDate TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    UserEmail TEXT NOT NULL
                )";
            command.ExecuteNonQuery();
            
            // Check and create BorrowedItems table if it doesn't exist
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS BorrowedItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ItemName TEXT NOT NULL,
                    Department TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    OfficialBorrowListRecordId INTEGER NOT NULL,
                    FOREIGN KEY (OfficialBorrowListRecordId) REFERENCES OfficialBorrowListRecords(Id)
                )";
            command.ExecuteNonQuery();
            
            // Check and create VerificationCodes table if it doesn't exist
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS VerificationCodes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Email TEXT NOT NULL,
                    Code TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    ExpiresAt TEXT NOT NULL,
                    IsUsed INTEGER NOT NULL DEFAULT 0
                )";
            command.ExecuteNonQuery();
            
            // Check and create TempSignups table if it doesn't exist
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS TempSignups (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Email TEXT NOT NULL,
                    Username TEXT NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    Role TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    StudentNumber TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL
                )";
            command.ExecuteNonQuery();
            
            // Check and create BorrowForms table if it doesn't exist
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS BorrowForms (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ReferenceCode TEXT NOT NULL,
                    StudentName TEXT NOT NULL,
                    StudentNumber TEXT NOT NULL,
                    StudentEmail TEXT NOT NULL,
                    ProfessorEmail TEXT NOT NULL,
                    SubjectCode TEXT NOT NULL,
                    SubmittedAt TEXT NOT NULL,
                    IsApproved INTEGER,
                    ProcessedAt TEXT,
                    ProcessedBy TEXT NOT NULL DEFAULT '',
                    RejectionReason TEXT NOT NULL DEFAULT '',
                    IsIssued INTEGER NOT NULL DEFAULT 0,
                    IsReturned INTEGER NOT NULL DEFAULT 0
                )";
            command.ExecuteNonQuery();
            
            // Add ProcessedBy and RejectionReason columns if they don't exist (for existing databases)
            try
            {
                command.CommandText = "PRAGMA table_info(BorrowForms)";
                var reader = command.ExecuteReader();
                var columns = new List<string>();
                while (reader.Read())
                {
                    columns.Add(reader.GetString(1)); // Column name is at index 1
                }
                reader.Close();
                
                if (!columns.Contains("ProcessedBy"))
                {
                    command.CommandText = "ALTER TABLE BorrowForms ADD COLUMN ProcessedBy TEXT NOT NULL DEFAULT ''";
                    command.ExecuteNonQuery();
                    Console.WriteLine("✅ Added ProcessedBy column to BorrowForms table");
                }
                
                if (!columns.Contains("RejectionReason"))
                {
                    command.CommandText = "ALTER TABLE BorrowForms ADD COLUMN RejectionReason TEXT NOT NULL DEFAULT ''";
                    command.ExecuteNonQuery();
                    Console.WriteLine("✅ Added RejectionReason column to BorrowForms table");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error checking/adding BorrowForms columns: {ex.Message}");
            }
            
            // Check and create BorrowFormItems table if it doesn't exist
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS BorrowFormItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Department TEXT NOT NULL,
                    ItemName TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    BorrowFormId INTEGER NOT NULL,
                    FOREIGN KEY (BorrowFormId) REFERENCES BorrowForms(Id) ON DELETE CASCADE
                )";
            command.ExecuteNonQuery();
            
            // Check and create InventoryItems table if it doesn't exist
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS InventoryItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Department TEXT NOT NULL,
                    ItemName TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    TotalQuantity INTEGER NOT NULL,
                    AvailableQuantity INTEGER NOT NULL,
                    MaxPerStudent INTEGER NOT NULL,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL,
                    LastUpdated TEXT NOT NULL
                )";
            command.ExecuteNonQuery();
        }
        
        connection.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Database setup warning: {ex.Message}");
    }
    
    // Remove "Volumetric Flask" from Chemistry department if it exists (legacy item)
    var volumetricFlask = await dbContext.InventoryItems
        .FirstOrDefaultAsync(i => i.Department == "CHEMISTRY LABORATORY TOOLROOM" && i.ItemName == "Volumetric Flask");
    if (volumetricFlask != null)
    {
        dbContext.InventoryItems.Remove(volumetricFlask);
        await dbContext.SaveChangesAsync();
        Console.WriteLine("✅ Removed legacy 'Volumetric Flask' from Chemistry department");
    }
    
    // Seed default admin and professor accounts
    await SeedDefaultAdminAccountsAsync(dbContext);
    await SeedDefaultProfessorAccountsAsync(dbContext);
    
    // Seed inventory items
    await SeedInventoryItemsAsync(dbContext);
}

// Function to seed default admin accounts
static async Task SeedDefaultAdminAccountsAsync(AppDbContext dbContext)
{
    try
    {
        // List of default admin accounts
        var defaultAdmins = new[]
        {
            new { Username = "admin1", Email = "admin1@ue.edu.ph", Password = "Admin123!", Name = "Administrator 1" },
            new { Username = "admin2", Email = "admin2@ue.edu.ph", Password = "Admin123!", Name = "Administrator 2" },
            new { Username = "admin3", Email = "admin3@ue.edu.ph", Password = "Admin123!", Name = "Administrator 3" },
            new { Username = "admin4", Email = "admin4@ue.edu.ph", Password = "Admin123!", Name = "Administrator 4" },
            new { Username = "admin5", Email = "admin5@ue.edu.ph", Password = "Admin123!", Name = "Administrator 5" }
        };

        foreach (var admin in defaultAdmins)
        {
            // Check if admin already exists
            var exists = await dbContext.Users
                .AnyAsync(u => u.Email.ToLower() == admin.Email.ToLower() || 
                              u.Username.ToLower() == admin.Username.ToLower());
            
            if (!exists)
            {
                // Get Philippines time (UTC+8)
                var philippinesTime = SecurityHelper.GetPhilippinesTime();

                var newAdmin = new Group5.Models.User
                {
                    Username = admin.Username,
                    Email = admin.Email,
                    PasswordHash = SecurityHelper.HashPassword(admin.Password), // Hash password
                    Role = "Administrator",
                    Name = admin.Name,
                    StudentNumber = string.Empty,
                    CreatedAtUtc = philippinesTime,
                    IsEmailVerified = true, // Admin accounts are pre-verified
                    FailedLoginAttempts = 0,
                    LockedUntil = null
                };
                
                dbContext.Users.Add(newAdmin);
                Console.WriteLine($"✅ Created default admin account: {admin.Username} ({admin.Email})");
            }
        }
        
        await dbContext.SaveChangesAsync();
        Console.WriteLine("✅ Default admin accounts seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error seeding admin accounts: {ex.Message}");
    }
}

// Function to seed default professor accounts
static async Task SeedDefaultProfessorAccountsAsync(AppDbContext dbContext)
{
    try
    {
        // Remove old placeholder professor accounts (prof1-prof5)
        var oldProfessors = await dbContext.Users
            .Where(u => u.Role == "Professor" && 
                       (u.Username == "prof1" || u.Username == "prof2" || 
                        u.Username == "prof3" || u.Username == "prof4" || 
                        u.Username == "prof5"))
            .ToListAsync();
        
        if (oldProfessors.Any())
        {
            dbContext.Users.RemoveRange(oldProfessors);
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"🗑️ Removed {oldProfessors.Count} old placeholder professor accounts (prof1-prof5)");
        }
        
        // List of default professor accounts (University of the East Faculty)
        var defaultProfessors = new[]
        {
            new { Username = "john.trinidad", Email = "johnvincent.trinidad@ue.edu.ph", Password = "UEProf2025!", Name = "Engr. John Vincent Trinidad" },
            new { Username = "nelson.rodelas", Email = "nelson.rodelas@ue.edu.ph", Password = "UEProf2025!", Name = "Dr. Nelson Rodelas" },
            new { Username = "joan.lazaro", Email = "joan.lazaro@ue.edu.ph", Password = "UEProf2025!", Name = "Dr. Joan Lazaro" },
            new { Username = "regie.david", Email = "regie.david@ue.edu.ph", Password = "UEProf2025!", Name = "Engr. Regie David" },
            new { Username = "arriane.cabreros", Email = "arriane.cabreros@ue.edu.ph", Password = "UEProf2025!", Name = "Engr. Arriane Cabreros" },
            new { Username = "alexis.rubio", Email = "alexisjohn.rubio@ue.edu.ph", Password = "UEProf2025!", Name = "Dr. Alexis John Rubio" },
            new { Username = "paraluman.sim", Email = "paraluman.sim@ue.edu.ph", Password = "UEProf2025!", Name = "Dr. Paraluman Sim" },
            new { Username = "ryan.francisco", Email = "ryan.francisco@ue.edu.ph", Password = "UEProf2025!", Name = "Engr. Ryan Francisco" },
            new { Username = "benedict.zurbito", Email = "benedict.zurbito@ue.edu.ph", Password = "UEProf2025!", Name = "Engr. Benedict Zurbito" },
            new { Username = "marjon.umbay", Email = "marjon.umbay@ue.edu.ph", Password = "UEProf2025!", Name = "Engr. Marjon Umbay" },
            new { Username = "ronnel.agulto", Email = "ronnel.agulto@ue.edu.ph", Password = "UEProf2025!", Name = "Engr. Ronnel Agulto" },
            new { Username = "kent.aglibar", Email = "kentdarryl.aglibar@ue.edu.ph", Password = "UEProf2025!", Name = "Engr. Kent Darryl Aglibar" }
        };

        foreach (var prof in defaultProfessors)
        {
            // Check if professor already exists
            var exists = await dbContext.Users
                .AnyAsync(u => u.Email.ToLower() == prof.Email.ToLower() || 
                              u.Username.ToLower() == prof.Username.ToLower());
            
            if (!exists)
            {
                // Get Philippines time (UTC+8)
                var philippinesTime = SecurityHelper.GetPhilippinesTime();

                var newProfessor = new Group5.Models.User
                {
                    Username = prof.Username,
                    Email = prof.Email,
                    PasswordHash = SecurityHelper.HashPassword(prof.Password), // Hash password
                    Role = "Professor",
                    Name = prof.Name,
                    StudentNumber = string.Empty,
                    CreatedAtUtc = philippinesTime,
                    IsEmailVerified = true, // Professor accounts are pre-verified
                    FailedLoginAttempts = 0,
                    LockedUntil = null
                };
                
                dbContext.Users.Add(newProfessor);
                Console.WriteLine($"✅ Created default professor account: {prof.Username} ({prof.Email})");
            }
        }
        
        await dbContext.SaveChangesAsync();
        Console.WriteLine("✅ Default professor accounts seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error seeding professor accounts: {ex.Message}");
    }
}

// Function to seed inventory items for all toolrooms
static async Task SeedInventoryItemsAsync(AppDbContext dbContext)
{
    try
    {
        // Remove "Volumetric Flask" from Chemistry department if it exists (legacy item)
        var volumetricFlask = await dbContext.InventoryItems
            .FirstOrDefaultAsync(i => i.Department == "CHEMISTRY LABORATORY TOOLROOM" && i.ItemName == "Volumetric Flask");
        if (volumetricFlask != null)
        {
            dbContext.InventoryItems.Remove(volumetricFlask);
            await dbContext.SaveChangesAsync();
            Console.WriteLine("✅ Removed legacy 'Volumetric Flask' from Chemistry department");
        }
        
        // Check if inventory already has items
        if (await dbContext.InventoryItems.AnyAsync())
        {
            Console.WriteLine("ℹ️ Inventory items already exist. Skipping seed.");
            return;
        }

        var now = DateTime.Now;
        var inventoryItems = new List<Group5.Models.InventoryItem>();

        // CIVIL ENGINEERING TOOLROOM
        inventoryItems.AddRange(new[]
        {
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Measuring Tape", Description = "Flexible retractable tape measure for distance and dimensional measurements. Essential for site work, layout, and verification of structural dimensions. Available in various lengths (3m-50m). Ensure tape is fully retracted after use.", TotalQuantity = 22, AvailableQuantity = 22, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Level Tool", Description = "Spirit level instrument for checking horizontal and vertical alignment in construction. Features bubble vials filled with liquid for precise leveling. Critical for ensuring structural integrity and proper installation of building components.", TotalQuantity = 16, AvailableQuantity = 16, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Compass", Description = "Magnetic compass for navigation, orientation, and bearing measurements in surveying and site work. Essential for mapping and establishing reference directions. Keep away from metal objects and magnetic interference.", TotalQuantity = 14, AvailableQuantity = 14, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Theodolite", Description = "Precision optical instrument for measuring horizontal and vertical angles in surveying. Used for triangulation, traversing, and establishing control points. Must be mounted on tripod and carefully leveled before use. Handle with extreme care.", TotalQuantity = 6, AvailableQuantity = 6, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Tripod", Description = "Three-legged adjustable stand providing stable support for surveying instruments like theodolites, levels, and total stations. Features extendable legs with locking mechanisms. Ensure all legs are firmly secured before mounting equipment.", TotalQuantity = 12, AvailableQuantity = 12, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Survey Rod", Description = "Graduated measuring rod (also called leveling staff) used with levels to determine differences in elevation. Features clear metric markings. Must be held vertically for accurate readings. Essential for topographic surveys and site grading.", TotalQuantity = 20, AvailableQuantity = 20, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Plumb Bob", Description = "Weight suspended on string used to establish true vertical reference lines. Essential for column alignment, wall plumbness verification, and transferring points vertically. Ensure bob hangs freely without obstructions.", TotalQuantity = 18, AvailableQuantity = 18, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Trowel", Description = "Handheld tool with flat blade for applying, spreading, and smoothing concrete, mortar, and plaster. Various shapes available for different applications. Clean thoroughly after each use to prevent hardening of materials.", TotalQuantity = 25, AvailableQuantity = 25, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Hammer", Description = "Basic construction tool for driving nails, breaking objects, and demolition work. Features steel head and wooden or fiberglass handle. Wear safety glasses when using. Inspect head for cracks before use.", TotalQuantity = 20, AvailableQuantity = 20, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Shovel", Description = "Manual digging tool for excavation, material transfer, and site preparation. Features flat or pointed blade depending on application. Essential for soil sampling and small-scale earthwork. Check handle integrity before use.", TotalQuantity = 15, AvailableQuantity = 15, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Safety Helmet", Description = "Hard hat providing head protection from falling objects and overhead hazards. Meets ANSI/ISEA safety standards. Mandatory personal protective equipment for all construction site visits. Adjust suspension system for secure fit.", TotalQuantity = 30, AvailableQuantity = 30, MaxPerStudent = 4, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Measuring Whee_", Description = "Rolling wheel device for measuring long distances on roads, paths, and large construction sites. Features counter display showing accumulated distance. More efficient than tape measures for distances over 30 meters.", TotalQuantity = 10, AvailableQuantity = 10, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Clinometer", Description = "Handheld instrument for measuring angles of slope, elevation, and inclination. Used in forestry, geology, and civil engineering for determining terrain gradients. Essential for road design and drainage planning.", TotalQuantity = 8, AvailableQuantity = 8, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Ruler", Description = "Rigid straight-edge measuring tool marked with metric and/or imperial units. Used for precise linear measurements in drawings, layouts, and small-scale work. Available in various lengths (15cm-100cm).", TotalQuantity = 35, AvailableQuantity = 35, MaxPerStudent = 4, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Square Ruler", Description = "L-shaped or triangular tool for drawing and verifying right angles (90°). Essential for layout work, formwork construction, and carpentry. Aluminum or steel construction ensures accuracy and durability.", TotalQuantity = 28, AvailableQuantity = 28, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Chalk Line", Description = "String coated with chalk powder used to mark long straight lines on surfaces. Essential for layout work in construction and carpentry. Snap string against surface to create visible reference line. Refill with chalk powder as needed.", TotalQuantity = 24, AvailableQuantity = 24, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CIVIL ENGINEERING TOOLROOM", ItemName = "Dumpy Level", Description = "Fixed telescope optical leveling instrument mounted on rotating tripod base. Used to establish horizontal plane for determining elevation differences and transferring heights. Requires careful leveling before use. Classic surveying equipment.", TotalQuantity = 7, AvailableQuantity = 7, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now }
        });

        // ELECTRONICS ENGINEERING TOOLROOM
        inventoryItems.AddRange(new[]
        {
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Multimeter", Description = "Digital multimeter for measuring voltage (AC/DC), current, resistance, and continuity in electronic circuits. Essential for troubleshooting and circuit analysis. Features auto-ranging and digital display. Always verify settings before connecting to circuit.", TotalQuantity = 20, AvailableQuantity = 20, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Oscilloscope", Description = "Electronic test instrument that displays voltage signals as waveforms over time. Essential for analyzing signal characteristics, frequency, and circuit behavior. Features multiple channels and triggering options. Requires training before use.", TotalQuantity = 8, AvailableQuantity = 8, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Function Generator", Description = "Signal generator producing various waveforms (sine, square, triangle, sawtooth) at adjustable frequencies and amplitudes. Used for circuit testing and signal injection. Features frequency sweep and modulation capabilities.", TotalQuantity = 10, AvailableQuantity = 10, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Power Supply", Description = "Adjustable DC power supply with voltage and current regulation for powering electronic circuits. Features short-circuit protection and digital displays. Always start with voltage set to zero and gradually increase.", TotalQuantity = 15, AvailableQuantity = 15, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Breadboard", Description = "Solderless prototyping board for building and testing electronic circuits. Features multiple connection points organized in rows and columns. No soldering required - components plug directly in. Reusable for multiple projects.", TotalQuantity = 30, AvailableQuantity = 30, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Soldering Iron", Description = "Temperature-controlled electric tool for joining electronic components with solder. Essential for permanent circuit assembly. Heat range 300-450°C. Always use proper ventilation and wear safety glasses. Return to stand when not in use.", TotalQuantity = 18, AvailableQuantity = 18, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "IC Chip", Description = "Integrated circuit packages containing various electronic functions (logic gates, amplifiers, microcontrollers). Handle with care to avoid ESD damage. Store in anti-static packaging. Verify pin configuration before use.", TotalQuantity = 6, AvailableQuantity = 6, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Jumper Wires", Description = "Pre-cut insulated wires with connector ends for breadboard and prototype connections. Available in various lengths and colors for organized circuit building. Color-coding helps identify power, ground, and signal connections.", TotalQuantity = 25, AvailableQuantity = 25, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Resistor", Description = "Passive electronic components that limit current flow and divide voltage. Available in various resistance values (color-coded bands). Power ratings from 1/4W to 2W. Essential for almost every circuit design.", TotalQuantity = 40, AvailableQuantity = 40, MaxPerStudent = 5, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Capacitor", Description = "Electronic component that stores and releases electrical energy. Available in various capacitances and voltage ratings. Types include ceramic, electrolytic, and film. Observe polarity on electrolytic capacitors.", TotalQuantity = 35, AvailableQuantity = 35, MaxPerStudent = 4, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Diode", Description = "Semiconductor device allowing current flow in one direction only. Used for rectification, voltage protection, and signal processing. Observe polarity markings (cathode band). Verify voltage and current ratings.", TotalQuantity = 8, AvailableQuantity = 8, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Transistor", Description = "Semiconductor device for amplification and switching applications. Available as BJT (bipolar) and FET types. Three terminals: base/gate, collector/drain, emitter/source. Essential building block of modern electronics.", TotalQuantity = 15, AvailableQuantity = 15, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "LED", Description = "Light Emitting Diode that produces light when current flows through it. Available in various colors and sizes. Observe polarity (longer lead is positive). Always use current-limiting resistor to prevent burnout.", TotalQuantity = 30, AvailableQuantity = 30, MaxPerStudent = 4, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Protoboard", Description = "Perforated board for permanent circuit assembly by soldering. Features copper pads in various patterns (stripboard, perfboard). Plan layout before soldering. Check connections for shorts before powering.", TotalQuantity = 20, AvailableQuantity = 20, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Switch", Description = "Mechanical or electronic device for opening or closing electrical circuits. Types include toggle, push-button, slide, and tactile switches. Verify voltage and current ratings for your application.", TotalQuantity = 25, AvailableQuantity = 25, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Relay", Description = "Electrically operated switch using electromagnetic coil to control high-power circuits with low-power signals. Features contacts rated for various voltages/currents. Essential for interfacing circuits. Check coil voltage before use.", TotalQuantity = 12, AvailableQuantity = 12, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRONICS ENGINEERING TOOLROOM", ItemName = "Inductor", Description = "Passive component that stores energy in magnetic field when current flows through it. Used in filters, transformers, and oscillators. Measured in henries (H). Consider DC resistance and current rating.", TotalQuantity = 18, AvailableQuantity = 18, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now }
        });

        // CHEMISTRY LABORATORY TOOLROOM
        inventoryItems.AddRange(new[]
        {
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Alcohol Lamp", Description = "Portable heat source using denatured alcohol as fuel. Produces clean, controllable flame for heating small quantities. Safer than open flames for some applications. Ensure wick is properly trimmed. Extinguish by covering, never blow out.", TotalQuantity = 15, AvailableQuantity = 15, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Balance Scale", Description = "Precision analytical balance for measuring mass to high accuracy (typically 0.001g or better). Essential for quantitative analysis. Level before use. Never place chemicals directly on pan - use weighing paper or containers. Calibrate regularly.", TotalQuantity = 8, AvailableQuantity = 8, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Beaker", Description = "Cylindrical glass container with flat bottom and spout for mixing, heating, and measuring liquids. Available in various sizes (50ml-2000ml). Not for precise volume measurements. Use with wire gauze when heating. Handle with care to prevent breakage.", TotalQuantity = 30, AvailableQuantity = 30, MaxPerStudent = 4, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Bunsen Burner", Description = "Gas-fueled burner producing adjustable flame for heating and sterilization. Features air control for flame type (safety flame vs. blue flame). Connect to gas source securely. Always light with air hole closed, then adjust. Turn off gas when not in use.", TotalQuantity = 12, AvailableQuantity = 12, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Burette", Description = "Long graduated glass tube with stopcock for precise dispensing of liquids, especially in titrations. Mounted on stand with clamp. Read meniscus at eye level for accuracy. Essential for volumetric analysis and acid-base titrations.", TotalQuantity = 18, AvailableQuantity = 18, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Crucible", Description = "High-temperature resistant ceramic or metal container for heating substances to very high temperatures. Used with crucible tongs. Essential for decomposition reactions, ash determination, and high-temperature experiments. Handle only when cool.", TotalQuantity = 16, AvailableQuantity = 16, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Evaporating Dish", Description = "Shallow ceramic or glass dish for evaporating solutions to concentrate or crystallize solutes. Wide surface area promotes evaporation. Can withstand moderate heating. Essential for crystallization and concentration experiments.", TotalQuantity = 20, AvailableQuantity = 20, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Flask", Description = "Erlenmeyer or round-bottom flask for mixing, heating, and storing chemical solutions. Made of borosilicate glass resistant to thermal shock. Available in various sizes (50ml-2000ml). Essential for chemical reactions and solution preparation. Clean thoroughly after use.", TotalQuantity = 25, AvailableQuantity = 25, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Funnel", Description = "Conical glass or plastic device for transferring liquids and filtering solutions. Prevents spills and aids in controlled pouring. Available in various sizes. Use with filter paper for filtration. Essential for safe liquid transfer and separation techniques.", TotalQuantity = 32, AvailableQuantity = 32, MaxPerStudent = 4, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Graduated Cylinder", Description = "Tall cylindrical glass container with volume markings for measuring liquid volumes. More accurate than beakers. Read meniscus at eye level. Available in various sizes (10ml-1000ml). Essential for volumetric measurements in experiments.", TotalQuantity = 35, AvailableQuantity = 35, MaxPerStudent = 4, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Litmus Paper", Description = "pH indicator strips that change color to indicate acidity or basicity of solutions. Red turns blue in base, blue turns red in acid. Quick qualitative pH testing. Store in sealed container to prevent moisture. Single-use strips.", TotalQuantity = 50, AvailableQuantity = 50, MaxPerStudent = 10, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Ph Meter", Description = "Digital electronic instrument for precise pH measurement of solutions. Features electrode probe and digital display. Calibrate with standard buffer solutions before use. Rinse electrode between measurements. Essential for accurate pH determination.", TotalQuantity = 10, AvailableQuantity = 10, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Pipette", Description = "Precision glass tube for transferring specific volumes of liquids. Available as volumetric (single volume) or graduated (multiple volumes). Use pipette bulb or pump - never mouth pipetting. Essential for accurate quantitative analysis and dilutions.", TotalQuantity = 22, AvailableQuantity = 22, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Test Tube", Description = "Small cylindrical glass tube for holding small quantities of chemicals for reactions and observations. Available in various sizes (10ml-25ml). Use test tube holder when heating. Never point open end toward yourself or others. Essential for qualitative analysis.", TotalQuantity = 40, AvailableQuantity = 40, MaxPerStudent = 5, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Thermometer", Description = "Temperature measurement device with mercury or alcohol-filled glass tube. Range typically -10°C to 110°C. Read at eye level. Never use as stirring rod. Handle carefully - mercury is toxic if broken. Essential for monitoring reaction temperatures.", TotalQuantity = 28, AvailableQuantity = 28, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "CHEMISTRY LABORATORY TOOLROOM", ItemName = "Wire Gauze", Description = "Metal mesh screen placed between glassware and heat source to distribute heat evenly and prevent direct flame contact. Prevents thermal stress and breakage. Place on tripod or ring stand. Essential safety equipment for heating glassware.", TotalQuantity = 20, AvailableQuantity = 20, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now }
        });

        // ELECTRICAL ENGINEERING TOOLROOM
        inventoryItems.AddRange(new[]
        {
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Multimeter", Description = "Digital multimeter for measuring AC/DC voltage, current, resistance, and continuity in electrical circuits. Essential for troubleshooting and circuit analysis. Features auto-ranging and safety ratings. Always verify settings and use appropriate range. Never exceed voltage/current ratings.", TotalQuantity = 18, AvailableQuantity = 18, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Clamp Meter", Description = "Current measurement tool that clamps around wire to measure current without breaking circuit. Non-invasive measurement for AC/DC current. Essential for troubleshooting live circuits safely. Verify current rating before use. Never clamp around multiple wires simultaneously.", TotalQuantity = 10, AvailableQuantity = 10, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Voltage Tester", Description = "Non-contact voltage detector for quickly checking if wires or outlets are live. Pen-style device that lights up or beeps near voltage. Essential safety tool before working on circuits. Test on known live source first to verify operation. Never rely solely on this tool.", TotalQuantity = 25, AvailableQuantity = 25, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Wire Stripper", Description = "Hand tool for removing insulation from electrical wires without damaging conductor. Features adjustable jaws for various wire gauges. Essential for making connections. Match stripper size to wire gauge. Pull straight to avoid nicking wire.", TotalQuantity = 20, AvailableQuantity = 20, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Circuit Breaker", Description = "Automatic overcurrent protection device that trips when current exceeds rated value. Protects circuits from overload and short circuits. Available in various current ratings (5A-100A). Essential for electrical safety. Never bypass or modify circuit breakers.", TotalQuantity = 15, AvailableQuantity = 15, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Insulation Tester", Description = "Megohmmeter for testing insulation resistance of wires, cables, and electrical equipment. Applies high voltage to detect insulation breakdown. Essential for preventive maintenance. Follow safety procedures - equipment under test may be energized.", TotalQuantity = 8, AvailableQuantity = 8, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Ammeter", Description = "Instrument for measuring electric current flow in amperes. Available as analog or digital. Must be connected in series with circuit. Verify current rating before use. Essential for circuit analysis and troubleshooting. Never connect in parallel - will cause short circuit.", TotalQuantity = 12, AvailableQuantity = 12, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Voltmeter", Description = "Instrument for measuring electrical potential difference (voltage) between two points. Available as analog or digital. Connect in parallel with circuit. Verify voltage rating before use. Essential for troubleshooting and circuit verification.", TotalQuantity = 14, AvailableQuantity = 14, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Ohmmeter", Description = "Instrument for measuring electrical resistance in ohms. Part of multimeter or standalone unit. Never measure resistance in live circuit. Disconnect power and discharge capacitors before measuring. Essential for continuity testing and component verification.", TotalQuantity = 16, AvailableQuantity = 16, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Wire Cutter", Description = "Hand tool with sharp jaws for cutting electrical wires cleanly. Available in various sizes for different wire gauges. Diagonal cutters (dikes) most common. Essential for wire preparation. Keep blades sharp for clean cuts. Use appropriate size for wire gauge.", TotalQuantity = 22, AvailableQuantity = 22, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Screwdriver Set", Description = "Collection of screwdrivers in various sizes and types (flathead, Phillips, Torx, etc.) for electrical work. Insulated handles for safety. Essential for installation and maintenance. Match screwdriver type to screw head. Use insulated tools when working with live circuits.", TotalQuantity = 30, AvailableQuantity = 30, MaxPerStudent = 4, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Safety Gloves", Description = "Insulated rubber gloves rated for electrical work. Provide protection from electric shock. Available in various voltage classes (Class 00-4). Essential personal protective equipment. Inspect for damage before each use. Never use damaged gloves.", TotalQuantity = 40, AvailableQuantity = 40, MaxPerStudent = 5, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Cable Ties", Description = "Plastic or nylon straps for organizing and securing wires and cables. Available in various lengths and strengths. Essential for neat wire management. Use appropriate size for bundle. Do not over-tighten - may damage insulation.", TotalQuantity = 50, AvailableQuantity = 50, MaxPerStudent = 10, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Cable Harness", Description = "Pre-assembled bundle of wires organized and secured together. Used in complex electrical systems for organized wiring. Reduces installation time and improves reliability. Verify wire colors and connections match specifications before use.", TotalQuantity = 18, AvailableQuantity = 18, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Terminal Block", Description = "Modular connector block for securely connecting multiple wires. Features screw terminals for reliable connections. Available in various sizes and configurations. Essential for panel wiring and distribution. Tighten screws securely but do not over-torque.", TotalQuantity = 24, AvailableQuantity = 24, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Fuse", Description = "One-time overcurrent protection device that melts when current exceeds rating. Available in various current ratings and types (fast-blow, slow-blow). Essential for circuit protection. Never replace with higher rating fuse. Always use correct type and rating.", TotalQuantity = 35, AvailableQuantity = 35, MaxPerStudent = 5, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "ELECTRICAL ENGINEERING TOOLROOM", ItemName = "Motor Starter", Description = "Electromagnetic device for starting and controlling electric motors. Includes overload protection and contactor. Essential for motor control applications. Verify voltage and current ratings match motor. Follow proper wiring diagrams.", TotalQuantity = 10, AvailableQuantity = 10, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now }
        });

        // PHYSICS LABORATORY TOOLROOM
        inventoryItems.AddRange(new[]
        {
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Meter Stick", Description = "Rigid measuring stick exactly one meter in length with centimeter and millimeter markings. Essential for measuring length, distance, and displacement in mechanics experiments. Read at eye level for accuracy. Handle carefully to maintain calibration.", TotalQuantity = 15, AvailableQuantity = 15, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Spring Balance", Description = "Force measurement device using spring extension to measure weight and force. Features scale in newtons or grams. Essential for force experiments and weight measurements. Zero before use. Do not exceed maximum capacity.", TotalQuantity = 10, AvailableQuantity = 10, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Pulley", Description = "Wheel with grooved rim for rope or cable to demonstrate mechanical advantage and simple machines. Available as single or multiple pulley systems. Essential for studying work, energy, and mechanical advantage. Ensure rope is properly seated in groove.", TotalQuantity = 20, AvailableQuantity = 20, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Inlcined Plane", Description = "Adjustable ramp for studying motion, acceleration, and friction. Features angle adjustment and smooth surface. Essential for studying Newton's laws and friction. Secure firmly before use. Measure angle accurately for calculations.", TotalQuantity = 8, AvailableQuantity = 8, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Pendulum", Description = "Weight suspended from fixed point for studying periodic motion and simple harmonic motion. Features adjustable length and bob mass. Essential for studying oscillations and wave motion. Measure length from pivot to center of mass.", TotalQuantity = 12, AvailableQuantity = 12, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Laser Pointer", Description = "Low-power laser producing coherent light beam for optics experiments. Essential for studying reflection, refraction, interference, and diffraction. Never point at eyes or people. Use appropriate power level for experiments. Essential for precision optics work.", TotalQuantity = 5, AvailableQuantity = 5, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Vernier Caliper", Description = "Precision measuring instrument for internal, external, and depth measurements. Features main scale and vernier scale for accuracy to 0.1mm or 0.01mm. Essential for precise dimensional measurements. Read carefully - main scale plus vernier reading.", TotalQuantity = 18, AvailableQuantity = 18, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Thermometer", Description = "Temperature measurement device with liquid-filled glass tube or digital display. Range typically -10°C to 110°C. Essential for thermodynamics experiments. Read at eye level. Never use as stirring rod. Handle carefully to prevent breakage.", TotalQuantity = 25, AvailableQuantity = 25, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "StopWatch", Description = "Precise time measurement device for measuring intervals in seconds and fractions. Digital or analog display. Essential for timing experiments and measuring periods. Start/stop/reset functions. Calibrate regularly for accuracy.", TotalQuantity = 10, AvailableQuantity = 10, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Spring Balance", Description = "Force measurement device using calibrated spring to measure weight and force. Features scale in newtons or grams. Essential for force experiments. Zero before use. Do not exceed maximum capacity. Handle carefully to maintain calibration.", TotalQuantity = 14, AvailableQuantity = 14, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Barometer", Description = "Instrument for measuring atmospheric pressure. Available as mercury or aneroid type. Essential for studying pressure, fluid mechanics, and weather. Read at eye level. Handle mercury barometers with extreme care - mercury is toxic.", TotalQuantity = 6, AvailableQuantity = 6, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Protractor", Description = "Semi-circular or circular tool for measuring and drawing angles. Marked in degrees (0-180° or 0-360°). Essential for geometry and angle measurements in optics and mechanics. Align center point with vertex of angle.", TotalQuantity = 30, AvailableQuantity = 30, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Magnet", Description = "Permanent magnet producing magnetic field for studying magnetism and electromagnetism. Available as bar, horseshoe, or disc magnets. Essential for magnetic field experiments. Keep away from electronic devices. Store with keepers to maintain strength.", TotalQuantity = 22, AvailableQuantity = 22, MaxPerStudent = 3, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Prism", Description = "Triangular glass or acrylic prism for studying light refraction, dispersion, and spectrum. Essential for optics experiments demonstrating white light separation into colors. Handle carefully - edges are sharp. Clean surfaces for clear results.", TotalQuantity = 8, AvailableQuantity = 8, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Lens set", Description = "Collection of convex and concave lenses of various focal lengths for optics experiments. Essential for studying refraction, image formation, and lens equations. Handle by edges only. Clean surfaces before use. Store in protective case.", TotalQuantity = 12, AvailableQuantity = 12, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Force Meter", Description = "Precise digital or analog instrument for measuring force in newtons. Features spring mechanism or load cell. Essential for force experiments and Newton's law demonstrations. Zero before use. Do not exceed maximum capacity.", TotalQuantity = 16, AvailableQuantity = 16, MaxPerStudent = 2, CreatedAt = now, LastUpdated = now },
            new Group5.Models.InventoryItem { Department = "PHYSICS LABORATORY TOOLROOM", ItemName = "Optical Bench", Description = "Long rail system with movable components for precise optics experiments. Features light source, lens holders, and screen holders. Essential for studying image formation, focal length, and optical systems. Align components carefully for accurate results.", TotalQuantity = 4, AvailableQuantity = 4, MaxPerStudent = 1, CreatedAt = now, LastUpdated = now }
        });

        // Add all items to database
        await dbContext.InventoryItems.AddRangeAsync(inventoryItems);
        await dbContext.SaveChangesAsync();

        Console.WriteLine($"✅ Seeded {inventoryItems.Count} inventory items across all toolrooms.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error seeding inventory items: {ex.Message}");
    }
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
