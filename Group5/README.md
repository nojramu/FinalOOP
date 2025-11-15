# 🏛️ University of the East - Engineering Borrowing System (UE-EBS)

A comprehensive laboratory equipment and tool borrowing management system built with ASP.NET Core Blazor Server for the **University of the East (UE)**.

A modern, user-friendly web application designed for managing toolroom inventory and equipment borrowing transactions. Students can browse items, create borrow requests, and track their borrowing history, while professors approve requests and administrators manage inventory, process borrows, and handle returns—all in real-time.

## 🎯 Overview

UE-EBS is an educational project demonstrating full-stack web development using ASP.NET Core Blazor Server and SQLite. It showcases essential concepts including:

- 🏗️ Entity Framework Core for database operations
- 🎨 Blazor Server for interactive, real-time UI
- 👥 Role-based access control (Student, Professor, Administrator)
- 💾 SQLite database with persistent data storage
- 🔐 User authentication with email verification
- 📧 Email service integration (Gmail SMTP)
- 🔒 Password hashing with BCrypt
- 🛡️ Account lockout security features
- 📊 Borrow form workflow management
- 🛒 Shopping cart functionality
- 📦 Inventory management across multiple toolrooms

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 9 SDK** or higher
- **Visual Studio Code** (recommended) or **Visual Studio 2022**
- **Git** (optional, for cloning)
- **Gmail account** (for email verification - configure in `appsettings.json`)

### Installation Steps

1. **Navigate to the project directory:**
   ```bash
   cd C:\Users\user\Group5
   ```

2. **Build the application:**
   ```bash
   dotnet build
   ```

3. **Configure Email Settings (Optional but Recommended):**
   
   Edit `appsettings.json` and update the email settings:
   ```json
   "EmailSettings": {
     "SmtpServer": "smtp.gmail.com",
     "SmtpPort": "587",
     "SmtpUsername": "your-email@gmail.com",
     "SmtpPassword": "your-app-password",
     "FromEmail": "your-email@gmail.com",
     "FromName": "University of the East - Borrowing System"
   }
   ```
   
   > **Note:** For Gmail, you'll need to generate an [App Password](https://support.google.com/accounts/answer/185833) if 2FA is enabled.

4. **Run the application:**
   ```bash
   dotnet run
   ```

5. **Open in your browser:**
   
   The application will output a URL (typically `http://localhost:5000` or `http://localhost:5295`)
   
   Copy and paste the URL into your web browser

6. **Access the application:**
   
   You'll be redirected to the Login page
   
   Use default admin/professor accounts or create a new student account

## 📱 User Roles & Features

### 🔐 Authentication

#### Sign-Up (Create New Student Account)

1. Click "Sign Up" on the login page
2. Fill in the following information:
   - **Username** - Unique identifier (min 3 characters, max 100)
   - **Email Address** - Valid email for account verification
   - **Full Name** - Your complete name (min 2 characters)
   - **Student Number** - Your student identification number
   - **Password** - Strong password (min 6 characters, must contain letter and number)
   - **Confirm Password** - Must match the password above
3. Click "Sign Up" button
4. **Email Verification Required:**
   - Check your email for verification code
   - Enter the code on the verification page
   - Account is activated after verification

> **Note:** Administrator and Professor accounts cannot be created through signup. They are pre-seeded in the database.

#### Login

1. Enter your **Username** and **Password**
2. Click "Login" button
3. You'll be redirected to your role-specific dashboard:
   - **Students** → Student Dashboard
   - **Professors** → Professor Dashboard
   - **Administrators** → Admin Dashboard

#### Forgot Password

1. Click "Forgot Password?" link on the login page
2. Enter your email address
3. Check your email for reset code
4. Enter the reset code
5. Create a new password

#### Account Security

- **Account Lockout:** After 5 failed login attempts, account is locked for 15 minutes
- **Password Hashing:** All passwords are hashed using BCrypt
- **Email Verification:** Required for all new student accounts

### 👨‍🎓 Student Portal

The student dashboard provides a complete borrowing management system.

#### 📊 Dashboard

- **Welcome page** with service overview
- **Status tracking:**
  - Pending forms awaiting professor approval
  - Currently borrowed items
  - Returned items history
- **Quick navigation** to key features
- **System announcements** from administrators

#### 🛒 Borrow Items

Create a new borrow request with these steps:

1. **Select Department/Toolroom:**
   - Civil Engineering Toolroom
   - Electronics Engineering Toolroom
   - Chemistry Laboratory Toolroom
   - Electrical Engineering Toolroom
   - Physics Laboratory Toolroom

2. **Browse Available Items:**
   - View item descriptions and specifications
   - Check available quantities
   - See maximum borrow limit per student

3. **Add Items to Cart:**
   - Select items and quantities
   - Respect maximum per-student limits
   - Add multiple items from different departments

4. **Submit Borrow Form:**
   - Enter **Subject Code** (e.g., CE101, ECE201)
   - Enter **Professor Email** (must be registered professor)
   - Review items in cart
   - Submit for professor approval

5. **Form Submission:**
   - Receive unique **Reference Code** for tracking
   - Form sent to professor for approval
   - Email notification sent to professor

#### 🛒 Cart

- View all items added to cart
- Adjust quantities before submission
- Remove items from cart
- Check availability before submitting

#### 📋 History

View all your past and current borrow requests:

- **Pending Forms** - Awaiting professor approval
- **Approved Forms** - Approved by professor, awaiting admin processing
- **Issued Items** - Currently borrowed items
- **Returned Items** - Completed borrows
- **Rejected Forms** - Forms rejected with reasons

Track detailed information:
- Reference Code and creation date
- Items requested (department, item name, quantity)
- Professor assigned
- Current status
- Processing dates

### 👨‍🏫 Professor Portal

Comprehensive form approval system for faculty members.

#### 📊 Dashboard

- **Overview statistics:**
  - Total pending forms
  - Approved forms count
  - Rejected forms count
  - Forms processed today

#### 📋 Pending Forms

Review and approve student borrow requests:

1. **View Form Details:**
   - Student information (name, student number, email)
   - Subject code
   - Items requested with quantities
   - Submission date

2. **Approve or Reject:**
   - **Approve:** Form sent to administrator for final processing
   - **Reject:** Provide rejection reason (optional)
   - Email notifications sent to student

3. **Form History:**
   - View all forms you've processed
   - Track approval/rejection history
   - Filter by status

### 👨‍💼 Administrator Portal

Complete system management for administrators.

#### 📊 System Overview

- **System statistics:**
  - Total users (students, professors, admins)
  - Total borrow forms
  - Pending approvals
  - Active borrows
  - Inventory status

- **Announcement Management:**
  - Post system-wide notices
  - Update announcements visible to all users

#### 📋 All Forms

Complete borrow form management:

- **View All Forms:**
  - Table displaying all borrow forms
  - Shows: Reference Code, Student Name, Subject Code, Professor, Status, Dates
  - Filter by status (Pending, Approved, Rejected, Issued, Returned)

- **Form Details:**
  - Student information and contact details
  - Items requested (department, item name, quantity)
  - Professor who approved
  - Current status and timeline

- **Form Actions:**
  - View detailed information
  - Process approved forms
  - Reject forms with reasons

#### 📦 Stock Management

Inventory management across all toolrooms:

- **View Inventory:**
  - Browse items by department
  - See total quantity, available quantity
  - Check maximum per-student limits
  - View item descriptions

- **Add New Items:**
  - Select department/toolroom
  - Enter item name and description
  - Set total quantity and max per student
  - Activate/deactivate items

- **Update Inventory:**
  - Modify item quantities
  - Update descriptions
  - Adjust max per-student limits
  - Mark items as active/inactive

#### 📤 Issue Items

Process approved borrow forms:

1. **View Approved Forms:**
   - Forms approved by professors
   - Ready for item issuance

2. **Issue Items:**
   - Verify item availability
   - Update inventory quantities
   - Mark form as "Issued"
   - Record issue date and admin

3. **Official Borrow List:**
   - Generate official borrow records
   - Track issued items
   - Maintain audit trail

#### 🔄 Returns

Handle item returns:

1. **View Issued Items:**
   - Currently borrowed items
   - Student information
   - Items borrowed

2. **Process Returns:**
   - Verify returned items
   - Update inventory (restore quantities)
   - Mark form as "Returned"
   - Record return date

3. **Return History:**
   - Track all returns
   - View return dates
   - Monitor item condition

#### 👥 View Users

User management and database viewer:

- **View All Users:**
  - Students, Professors, Administrators
  - User details (name, email, username, role)
  - Account creation dates
  - Email verification status

- **User Statistics:**
  - Total users by role
  - Active accounts
  - Verified accounts

## 🗄️ Database Structure

### SQLite Database

- **Location:** `app.db` in the application directory
- **Type:** SQLite (file-based, no server required)
- **ORM:** Entity Framework Core 9.0

### Database Tables

#### Users Table

Stores user account information for authentication and profiles.

```sql
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL,
    Name TEXT NOT NULL,
    StudentNumber TEXT NOT NULL,
    CreatedAtUtc DATETIME NOT NULL,
    IsEmailVerified INTEGER NOT NULL DEFAULT 0,
    FailedLoginAttempts INTEGER NOT NULL DEFAULT 0,
    LockedUntil TEXT
);
```

| Column | Type | Description | Constraints |
|--------|------|-------------|-------------|
| Id | INTEGER | Unique user identifier | PRIMARY KEY, AUTO INCREMENT |
| Username | TEXT | Unique login identifier | NOT NULL, UNIQUE, Max 100 chars |
| Email | TEXT | Email address | NOT NULL, UNIQUE, Max 100 chars |
| PasswordHash | TEXT | BCrypt hashed password | NOT NULL, Max 255 chars |
| Role | TEXT | User role (Student/Professor/Administrator) | NOT NULL |
| Name | TEXT | User's full name | NOT NULL, Max 100 chars |
| StudentNumber | TEXT | Student ID (empty for non-students) | Max 50 chars |
| CreatedAtUtc | DATETIME | Account creation timestamp | NOT NULL |
| IsEmailVerified | INTEGER | Email verification status | NOT NULL, DEFAULT 0 |
| FailedLoginAttempts | INTEGER | Failed login counter | NOT NULL, DEFAULT 0 |
| LockedUntil | TEXT | Account lockout expiration | NULLABLE |

#### InventoryItems Table

Records all equipment and tools available in toolrooms.

```sql
CREATE TABLE InventoryItems (
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
);
```

| Column | Type | Description | Constraints |
|--------|------|-------------|-------------|
| Id | INTEGER | Unique item identifier | PRIMARY KEY, AUTO INCREMENT |
| Department | TEXT | Toolroom/department name | NOT NULL |
| ItemName | TEXT | Item name | NOT NULL |
| Description | TEXT | Item description | NOT NULL |
| TotalQuantity | INTEGER | Total stock quantity | NOT NULL, >= 0 |
| AvailableQuantity | INTEGER | Currently available quantity | NOT NULL, >= 0 |
| MaxPerStudent | INTEGER | Maximum borrow limit per student | NOT NULL, >= 1 |
| IsActive | INTEGER | Active status flag | NOT NULL, DEFAULT 1 |
| CreatedAt | DATETIME | Item creation timestamp | NOT NULL |
| LastUpdated | DATETIME | Last update timestamp | NOT NULL |

#### BorrowForms Table

Records all student borrow requests.

```sql
CREATE TABLE BorrowForms (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ReferenceCode TEXT NOT NULL UNIQUE,
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
);
```

| Column | Type | Description | Constraints |
|--------|------|-------------|-------------|
| Id | INTEGER | Unique form identifier | PRIMARY KEY, AUTO INCREMENT |
| ReferenceCode | TEXT | Unique reference code | NOT NULL, UNIQUE |
| StudentName | TEXT | Student's full name | NOT NULL |
| StudentNumber | TEXT | Student ID | NOT NULL |
| StudentEmail | TEXT | Student email | NOT NULL |
| ProfessorEmail | TEXT | Professor email | NOT NULL |
| SubjectCode | TEXT | Course/subject code | NOT NULL |
| SubmittedAt | DATETIME | Form submission timestamp | NOT NULL |
| IsApproved | INTEGER | Approval status (NULL/Pending, 1/Approved, 0/Rejected) | NULLABLE |
| ProcessedAt | DATETIME | Processing timestamp | NULLABLE |
| ProcessedBy | TEXT | Admin who processed | DEFAULT '' |
| RejectionReason | TEXT | Rejection reason if rejected | DEFAULT '' |
| IsIssued | INTEGER | Item issuance status | NOT NULL, DEFAULT 0 |
| IsReturned | INTEGER | Return status | NOT NULL, DEFAULT 0 |

#### BorrowFormItems Table

Items requested in each borrow form.

```sql
CREATE TABLE BorrowFormItems (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Department TEXT NOT NULL,
    ItemName TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    BorrowFormId INTEGER NOT NULL,
    FOREIGN KEY (BorrowFormId) REFERENCES BorrowForms(Id) ON DELETE CASCADE
);
```

#### CartItems Table

Temporary cart storage for students.

```sql
CREATE TABLE CartItems (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Department TEXT NOT NULL,
    ItemName TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    UserEmail TEXT NOT NULL
);
```

#### OfficialBorrowListRecords Table

Official records of issued items.

```sql
CREATE TABLE OfficialBorrowListRecords (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    StudentName TEXT NOT NULL,
    StudentNumber TEXT NOT NULL,
    ReferenceCode TEXT NOT NULL,
    ProfessorInCharge TEXT NOT NULL,
    BorrowDate TEXT NOT NULL,
    Status TEXT NOT NULL,
    UserEmail TEXT NOT NULL
);
```

#### BorrowedItems Table

Items associated with official borrow records.

```sql
CREATE TABLE BorrowedItems (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ItemName TEXT NOT NULL,
    Department TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    OfficialBorrowListRecordId INTEGER NOT NULL,
    FOREIGN KEY (OfficialBorrowListRecordId) REFERENCES OfficialBorrowListRecords(Id)
);
```

#### VerificationCodes Table

Email verification and password reset codes.

```sql
CREATE TABLE VerificationCodes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Email TEXT NOT NULL,
    Code TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    ExpiresAt TEXT NOT NULL,
    IsUsed INTEGER NOT NULL DEFAULT 0
);
```

#### TempSignups Table

Temporary storage for unverified signups.

```sql
CREATE TABLE TempSignups (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Email TEXT NOT NULL,
    Username TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL,
    Name TEXT NOT NULL,
    StudentNumber TEXT NOT NULL,
    CreatedAt TEXT NOT NULL
);
```

## 🏗️ Project Structure

```
Group5/
├── Components/
│   ├── _Imports.razor                  # Global component imports
│   ├── App.razor                       # Root application component
│   ├── Routes.razor                    # Application routing configuration
│   │
│   ├── Layout/
│   │   ├── MainLayout.razor            # Default layout for auth pages
│   │   └── MainLayout.razor.css        # Main layout styles
│   │
│   └── Pages/
│       ├── Error.razor                 # Error page (500, etc.)
│       │
│       ├── Auth/                       # Authentication pages
│       │   ├── Home.razor              # Login page
│       │   ├── Signup.razor            # Sign-up/registration page
│       │   ├── VerifyEmail.razor       # Email verification page
│       │   ├── ForgotPassword.razor    # Password recovery request
│       │   ├── VerifyResetCode.razor   # Reset code verification
│       │   └── ResetPassword.razor     # Password reset page
│       │
│       ├── Admin/                      # Admin-only pages
│       │   ├── AdminDashboard.razor    # Admin overview dashboard
│       │   ├── AdminForms.razor        # All borrow forms management
│       │   ├── AdminStock.razor        # Inventory management
│       │   ├── AdminIssue.razor        # Item issuance
│       │   ├── AdminReturns.razor      # Return processing
│       │   ├── OfficialBorrowList.razor # Official borrow records
│       │   ├── VerifyBorrowList.razor  # Borrow list verification
│       │   └── ViewDatabase.razor      # User database viewer
│       │
│       ├── Professor/                  # Professor-only pages
│       │   ├── ProfessorDashboard.razor # Professor overview
│       │   └── ProfessorForms.razor    # Form approval management
│       │
│       ├── Students/                   # Student-only pages
│       │   ├── StudentDashboard.razor  # Student overview
│       │   ├── StudentBorrow.razor     # Browse and borrow items
│       │   ├── Cart.razor              # Shopping cart
│       │   ├── StudentHistory.razor    # Borrow history
│       │   ├── CE.razor                # Civil Engineering toolroom
│       │   ├── ECE.razor               # Electronics Engineering toolroom
│       │   ├── EE.razor                # Electrical Engineering toolroom
│       │   ├── CHEM.razor              # Chemistry Laboratory toolroom
│       │   └── P6.razor                # Physics Laboratory toolroom
│       │
│       └── Shared/                     # Shared pages
│           ├── Aboutus.razor           # About page
│           ├── Help.razor              # Help and FAQ page
│           └── Error.razor             # Error page
│
├── Core/                                # Core application code (C# files)
│   ├── Data/
│   │   ├── AppDbContext.cs             # Entity Framework database context
│   │   └── Repositories/
│   │       ├── IRepository.cs          # Generic repository interface
│   │       ├── Repository.cs           # Generic repository implementation
│   │       ├── IInventoryRepository.cs # Inventory repository interface
│   │       └── InventoryRepository.cs  # Inventory repository implementation
│   │
│   ├── Models/
│   │   ├── User.cs                     # User entity
│   │   ├── InventoryItem.cs            # Inventory item entity
│   │   ├── BorrowForm.cs               # Borrow form entity
│   │   ├── BorrowFormItem.cs           # Borrow form item entity
│   │   ├── CartItem.cs                 # Cart item entity
│   │   ├── BorrowedItem.cs             # Borrowed item entity
│   │   ├── OfficialBorrowListRecord.cs # Official borrow record entity
│   │   ├── TempSignup.cs               # Temporary signup entity
│   │   └── VerificationCode.cs         # Verification code entity
│   │
│   ├── Services/
│   │   ├── EmailService.cs             # Email sending service
│   │   ├── Interfaces/
│   │   │   ├── IEmailService.cs        # Email service interface
│   │   │   └── IInventoryService.cs    # Inventory service interface
│   │   ├── InventoryService.cs         # Inventory management service
│   │   ├── Business/
│   │   │   ├── CartManager.cs          # Cart management logic
│   │   │   └── BorrowManager.cs        # Borrow workflow logic
│   │   └── Helpers/
│   │       └── SecurityHelper.cs       # Security utilities (hashing, validation)
│   │
│   ├── Shared/
│   │   ├── UserSession.cs              # User session management
│   │   └── UserDatabase.cs             # Static user database utilities
│   │
│   └── Base/                            # Base page classes
│       ├── BasePage.cs                 # Base page component
│       ├── BaseAdminPage.cs            # Admin page base class
│       └── BaseToolroomPage.cs         # Toolroom page base class
│
├── Properties/
│   └── launchSettings.json             # Application launch configuration
│
├── wwwroot/
│   ├── app.css                         # Global styles
│   ├── css/
│   │   ├── admin-modern.css            # Admin-specific styles
│   │   └── shared-auth.css             # Authentication page styles
│   └── images/
│       ├── UE.png                      # University logo
│       ├── ENLOGO.png                  # Engineering logo
│       └── Apparatus/                  # Equipment images
│           ├── Chemistry/
│           ├── Civil Engineering/
│           ├── Electrical/
│           ├── Electronics/
│           └── Physics/
│
├── Program.cs                          # Application startup configuration
├── Group5.csproj                       # Project file
├── appsettings.json                    # Application settings
├── appsettings.Development.json        # Development-specific settings
└── app.db                              # SQLite database file
```

## 🔧 Core Application Files

### Program.cs

Application startup and dependency injection configuration:

- Razor Components and Interactive Server Components
- SQLite database connection
- DbContext registration (AppDbContext)
- Email service registration
- Inventory service registration
- Repository pattern registration
- Business logic managers (CartManager, BorrowManager)
- Database initialization and seeding
- Default admin and professor accounts
- Inventory items seeding

### AppDbContext.cs (Data/AppDbContext.cs)

Entity Framework Core database context:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<BorrowForm> BorrowForms { get; set; }
    public DbSet<BorrowFormItem> BorrowFormItems { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<BorrowedItem> BorrowedItems { get; set; }
    public DbSet<OfficialBorrowListRecord> OfficialBorrowListRecords { get; set; }
    public DbSet<VerificationCode> VerificationCodes { get; set; }
    public DbSet<TempSignup> TempSignups { get; set; }
}
```

### Models

#### User.cs
Represents a user account (Student, Professor, or Administrator)
- Properties: Username, Email, PasswordHash, Role, Name, StudentNumber, CreatedAtUtc, IsEmailVerified, FailedLoginAttempts, LockedUntil
- Validations: Required fields, unique constraints

#### InventoryItem.cs
Represents equipment/tool in toolroom inventory
- Properties: Department, ItemName, Description, TotalQuantity, AvailableQuantity, MaxPerStudent, IsActive, CreatedAt, LastUpdated
- Tracks: Stock levels, availability, borrowing limits

#### BorrowForm.cs
Represents a student borrow request
- Properties: ReferenceCode, StudentName, StudentNumber, StudentEmail, ProfessorEmail, SubjectCode, SubmittedAt, IsApproved, ProcessedAt, ProcessedBy, RejectionReason, IsIssued, IsReturned
- Workflow: Pending → Approved/Rejected → Issued → Returned

#### BorrowFormItem.cs
Items requested in a borrow form
- Properties: Department, ItemName, Quantity, BorrowFormId
- Relationship: Many-to-one with BorrowForm

### Services

#### EmailService.cs
Email sending service using SMTP:
- Send verification codes for signup
- Send password reset codes
- Send form approval/rejection notifications
- Configured via appsettings.json

#### SecurityHelper.cs
Security utilities:
- Password hashing with BCrypt
- Password strength validation
- Email format validation
- Verification code generation
- Account lockout management
- Philippines timezone handling

#### CartManager.cs
Shopping cart management:
- Add items to cart
- Remove items from cart
- Update quantities
- Clear cart after form submission

#### BorrowManager.cs
Borrow workflow management:
- Form submission logic
- Inventory availability checking
- Quantity validation
- Reference code generation

## 🎨 Layout & Styling

### Layout Components

#### MainLayout.razor
Used for login, signup, and authentication pages:
- University branding (UE logo and colors)
- Navigation links (Home, Help, About Us)
- Responsive design

### Styling

- **Global Styles:** `wwwroot/app.css`
- **Admin Styles:** `wwwroot/css/admin-modern.css`
- **Auth Styles:** `wwwroot/css/shared-auth.css`
- **Framework:** Custom CSS with modern design
- **Color Scheme:** University of the East red (#8B0000)

## 🔐 Security Features

### Authentication

- Username/password-based login
- Unique username and email validation
- **BCrypt password hashing** (secure password storage)
- Email verification required for new accounts
- Account lockout after 5 failed attempts (15-minute lockout)

### Authorization

- Role-based access control (Student, Professor, Administrator)
- Page-level authorization checks
- Admin pages only accessible to administrators
- Professor pages only accessible to professors
- Student pages only accessible to students

### Data Validation

- Model-level validation via data annotations
- Client-side validation in forms
- Server-side validation on submit
- Password strength requirements (min 6 chars, letter + number)
- Email format validation
- Quantity limits enforcement

### Best Practices

- Unique constraints on username and email
- Password hashing with BCrypt
- Account lockout protection
- Email verification for account activation
- Secure session management
- SQL injection protection via EF Core parameterized queries

## 📊 Key Features

### 🛒 Borrow Management

- Browse items across 5 toolrooms
- Add items to cart
- Submit borrow forms with professor assignment
- Track form status in real-time
- View borrow history

### 📋 Form Workflow

1. **Student submits form** → Status: Pending
2. **Professor reviews** → Approve/Reject
3. **If approved** → Status: Approved (sent to admin)
4. **Admin processes** → Status: Issued
5. **Items returned** → Status: Returned

### 📦 Inventory Management

- Add/edit/remove inventory items
- Track total and available quantities
- Set maximum per-student limits
- Activate/deactivate items
- Multi-department support

### 💬 Email Notifications

- Signup verification codes
- Password reset codes
- Form approval notifications
- Form rejection notifications (with reasons)

### 🌐 Real-Time Updates

- Blazor Server provides live UI updates
- No page refresh needed
- Instant feedback from server
- Real-time status updates

## 🛠️ Technologies & Dependencies

### Core Framework

- **ASP.NET Core 9.0**
- **Blazor Server**
- **Entity Framework Core 9.0**

### Database

- **SQLite** (lightweight, file-based)

### Frontend

- **Razor Components**
- **HTML5 / CSS3**
- **JavaScript Interop** (for client-side interactions)

### Additional Libraries

- **Microsoft.EntityFrameworkCore.Sqlite** (9.0.10)
- **BCrypt.Net-Next** (4.0.3) - Password hashing

## 🚨 Troubleshooting

### Port Already in Use Error

If you see: `"Failed to bind to address http://127.0.0.1:5295: address already in use"`

**Solution:**

1. **Option 1:** Change the port in `launchSettings.json`
   - Edit `Properties/launchSettings.json` and change port to 5296 or 5297

2. **Option 2:** Kill the process using the port
   ```bash
   # Find the process ID (PID) using the port
   netstat -ano | findstr :5295
   
   # Kill the process (replace PID with actual process ID)
   taskkill /PID <PID> /F
   
   # Then run again
   dotnet run
   ```

### Database Issues

If the database appears corrupted:

```bash
# Delete the database file
Remove-Item app.db

# Run the application again
dotnet run

# This will recreate the database with proper schema and seed data
```

### Email Configuration Issues

If email verification is not working:

1. **Check appsettings.json:**
   - Verify SMTP settings are correct
   - For Gmail, use App Password (not regular password)
   - Enable "Less secure app access" or use App Password

2. **Test Email Service:**
   - Check console for email sending errors
   - Verify network connectivity
   - Check firewall settings

### Login Issues

- Ensure you created an account via the Sign-Up page (for students)
- Check that your username and password match exactly (case-sensitive)
- Verify email is verified (check email for verification code)
- Check if account is locked (wait 15 minutes after 5 failed attempts)
- Use default admin/professor accounts if available:
  - Admin: `admin1` / `Admin123!`
  - Professor: Check seeded accounts in Program.cs

### Form Submission Issues

- Ensure all required fields are filled
- Check that professor email is registered in system
- Verify item availability (check stock)
- Confirm quantities don't exceed max per-student limits
- Check that items are active in inventory

## 📚 Learning Objectives

This project is designed to teach:

### Web Development
- Building interactive web applications with Blazor
- Real-time updates without page refresh
- Server-side rendering with .NET
- Component-based architecture

### Object-Oriented Programming
- Entity models (User, InventoryItem, BorrowForm classes)
- Encapsulation and abstraction
- Inheritance (BasePage, BaseAdminPage)
- Polymorphism (virtual/override methods)
- Repository pattern

### Database Design
- Relational database structure
- Entity relationships (one-to-many, foreign keys)
- Data integrity and constraints
- ACID principles
- Database migrations

### Back-End Development
- Entity Framework Core ORM
- CRUD operations (Create, Read, Update, Delete)
- Repository pattern implementation
- Dependency injection
- Service layer architecture

### Software Engineering
- Project structure and organization
- Code reusability
- Error handling and logging
- User experience design
- Security best practices
- Email service integration

## 🎓 Best Practices Demonstrated

✅ Separation of concerns (Models, Data, Services, Components)  
✅ Dependency Injection (DI) container  
✅ Repository pattern for data access  
✅ Service layer for business logic  
✅ Model validation using data annotations  
✅ Role-based authorization  
✅ Error handling with try-catch blocks  
✅ User-friendly error messages  
✅ Real-time UI updates  
✅ Clean, readable code structure  
✅ Password hashing with BCrypt  
✅ Account lockout security  
✅ Email verification workflow  

## 📝 Development Notes

### Adding New Features

1. Create entity model in `Models/` folder
2. Add `DbSet` to `AppDbContext.cs`
3. Create Razor component in `Components/Pages/`
4. Update routes in `Routes.razor` (if needed)
5. Add business logic in `Services/` if needed
6. Test thoroughly before deployment

### Database Migrations (When Needed)

```bash
# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create migration
dotnet ef migrations add MigrationName

# Apply migration
dotnet ef database update
```

### Debugging

- Use Visual Studio Code debugger with C# extension
- Set breakpoints and step through code
- Check browser console for client-side errors
- Review application logs for server errors
- Use `Console.WriteLine()` for debugging output

## 🚀 Deployment

### Build for Production

```bash
dotnet build --configuration Release
```

### Publish Application

```bash
dotnet publish --configuration Release --output ./publish
```

### Run Published Version

```bash
cd publish
dotnet Group5.dll
```

## 📞 Support & Resources

### Documentation

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Blazor Server Documentation](https://docs.microsoft.com/aspnet/core/blazor)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [SQLite Documentation](https://www.sqlite.org/docs.html)
- [BCrypt.Net Documentation](https://github.com/BcryptNet/bcrypt.net)

### Common Issues

- Check the Troubleshooting section above
- Review application logs
- Verify all prerequisites are installed
- Ensure .NET SDK 9.0 or higher is installed
- Check email configuration in appsettings.json

## 📄 License

This project is for educational purposes. Use and modify as needed for learning.

## ✨ Future Enhancements

Potential features to add:

- 📧 Email notifications for form status changes
- 📊 Advanced reporting and analytics
- 🔔 Real-time notifications (SignalR)
- 📱 Mobile-responsive design improvements
- 🗓️ Due date tracking and reminders
- 📸 Item image uploads
- 🔍 Advanced search and filtering
- 📈 Usage statistics and reports
- 🌐 Multi-language support
- 🔄 Automatic inventory restocking alerts
- 📋 Bulk operations for admins
- 🎨 Customizable themes

## 🤝 Contributing

Feel free to fork, modify, and improve this project for educational purposes!

## 📅 Project Information

- **Framework:** ASP.NET Core 9.0 Blazor Server
- **Database:** SQLite
- **Language:** C# with Razor
- **Purpose:** Educational - Learning C# web development
- **Institution:** University of the East (UE)

---

Thank you for using UE-EBS!

*University of the East - Engineering Borrowing System*


