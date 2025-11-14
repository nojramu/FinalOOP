<p align="center">
  <img src="wwwroot/images/logo.png" alt="Water Station Online Appointment Logo" width="200">
</p>

---

# 💧 Water Station Online Appointment (WSOA)

> **A comprehensive water refill service management system built with ASP.NET Core Blazor Server**

A modern, user-friendly web application designed for managing online water refill transactions. Customers can place orders, track their delivery status, and provide feedback, while administrators can manage orders, process payments, and respond to customer inquiries—all in real-time.

---

## 🎯 Overview

**WSOA** is an educational project demonstrating full-stack web development using **ASP.NET Core Blazor Server** and **SQLite**. It showcases essential concepts including:

- 🏗️ **Entity Framework Core** for database operations
- 🎨 **Blazor Server** for interactive, real-time UI
- 👥 **Role-based access control** (Admin vs. Customer)
- 💾 **SQLite database** with persistent data storage
- 🔐 **User authentication** and account management
- 📊 **Order management** with status tracking
- 💬 **Feedback system** with admin replies

---

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **[.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)** or higher
- **Visual Studio Code** (recommended) or **Visual Studio 2022**
- **Git** (optional, for cloning)

### Installation Steps

1. **Clone or navigate to the project directory**:
   ```bash
   cd c:\Users
   ```

2. **Navigate to the application folder**:
   ```bash
   cd test
   ```

3. **Build the application**:
   ```bash
   dotnet build
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

5. **Open in your browser**:
   - The application will output a URL (typically `http://localhost:5000` or `http://localhost:5295`)
   - Copy and paste the URL into your web browser

6. **Access the application**:
   - You'll be redirected to the **Login/Signup** page
   - Create a new account or use existing credentials

---

## 📱 User Roles & Features

### 🔐 Authentication

#### Sign-Up (Create New Account)
1. Click **"Create Account"** on the login page
2. Fill in the following information:
   - **Full Name** - Your complete name
   - **Username** - Unique identifier (min 3 characters)
   - **Email Address** - Valid email for account recovery
   - **Contact Number** - Mobile number for order updates
   - **House Number** - Delivery address (house/building number)
   - **Street** - Street name
   - **Barangay** - Barangay in Navotas City
   - **Password** - Strong password (min 6 characters)
   - **Confirm Password** - Must match the password above
   - **Terms & Conditions** - Check to agree
3. Click **"Create Account"** button
4. Account is created and you're ready to log in

#### Login
1. Enter your **Username** and **Password**
2. Click **"Login"** button
3. You'll be redirected to your dashboard/homepage

#### Forgot Password (If Applicable)
- Use the **"Forgot Password?"** link to reset your password
- Follow the email recovery instructions

---

## 👤 Customer Portal

The customer dashboard provides a complete order management and feedback system.

### 📍 **Home**
- Welcome page with service overview
- Quick navigation to key features
- Promotional banners or announcements
- Information about water gallon types and pricing

### 🛒 **Place Order**
Create a new water refill order with these steps:

1. **Gallon Information**:
   - Answer: "Do you have an existing gallon?" (Yes/No)
   - Answer: "Do you want to purchase a gallon?" (Yes/No)

2. **Purchase Gallons** (if applicable):
   - Select gallon **type**: Slim or Round
   - Enter **quantity** to purchase
   - Each gallon comes with initial refill

3. **Refill Gallons** (if applicable):
   - Select gallon **type**: Slim or Round
   - Enter **quantity** to refill
   - Only for existing gallons

4. **Customer Information**:
   - Enter **customer name** to put on the order
   - Enter saved address or a new one
   - **House Number, Street, Barangay** (Navotas City)

5. **Payment Method**:
   - Select: **Cash** or **E-Wallet** (PayMaya, GCash, etc.)
   - Review **total price** before submission

6. **Submit Order**:
   - Click **"Place Order"** button
   - Order confirmation will be displayed
   - You'll receive an order ID for tracking

### 📋 **Order History**
- View all your past and current orders
- Track **order status**: Pending → Completed / Cancelled
- See **payment status**: Pending → Completed / Cancelled
- View detailed information:
  - Order ID and creation date
  - Items ordered (purchase/refill quantities)
  - Delivery address
  - Total amount paid
  - Current status
- Filter or search orders by date

### 💬 **Feedback**
Engage with the service by leaving feedback:

1. **Write Feedback**:
   - Enter your **feedback message** about the product/service
   - Share your experience and suggestions

2. **Rating System**:
   - Select a **rating** (1-5 stars)
   - Rate your overall experience

3. **Privacy Settings**:
   - Choose to make your **name hidden** for other users
   - Public feedback helps the business improve

4. **View Feedback**:
   - Read feedback from other customers
   - View admin replies to feedback
   - See helpful comments and suggestions

5. **Submit Feedback**:
   - Click **"Submit Feedback"** button
   - Your feedback is posted immediately
   - Admin will review and may reply

### 👤 **Account Settings**
- View and update **profile information**:
  - Full name
  - Email address
  - Contact number
  - Delivery address (house, street, barangay)
- Change **password**
- View **account creation date**
- Manage **privacy settings** (if applicable)

### ℹ️ **About Us**
- Information about the water station business
- Company mission and values
- Service details and availability
- Contact information

### 🚪 **Logout**
- Click **"Logout"** button (usually in navigation bar)
- You'll be returned to the login page
- Your session ends securely

---

## 👨‍💼 Admin Portal

Comprehensive order and customer management system for administrators.

### 📊 **Dashboard / Home**
Administrative overview and analytics:

- **Sales Summary**:
  - Total revenue for selected period
  - Number of orders processed
  - Order completion rate
  - Payment success rate

- **Quick Stats**:
  - Pending orders
  - Pending payments
  - New feedback awaiting replies
  - Active customers

- **Charts & Graphs** :
  - Order trends over time
  - Revenue 

### 📦 **Orders Management**
Complete order management and fulfillment system:

#### View All Orders
- Table displaying all customer orders
- Shows: Order ID, Customer Name, Order Date, Status, Payment Status, Total Amount

#### Order Details
Click on an order to view:
- Customer information and delivery address
- Items ordered (purchase/refill details)
- Payment method and total amount
- Current order status
- Current payment status
- Order timeline

#### Update Order Status
For each order, update the fulfillment status:

| Status | Description |
|--------|-------------|
| **Pending** | Order received, awaiting processing |
| **Picked Up** | Items ready and picked from warehouse |
| **Delivered** | Order delivered to customer |
| **Cancelled** | Order cancelled by admin or customer |
| **Completed** | Order fulfilled and closed |

#### Update Payment Status
Monitor and update payment processing:

| Status | Description |
|--------|-------------|
| **Pending** | Payment awaiting confirmation |
| **Completed** | Payment received and verified |
| **Refunded** | Payment refunded to customer |

### 💬 **Feedback Management**
Manage customer feedback and maintain service quality:

#### View All Feedback
- List of customer feedback with ratings
- Shows customer name (or "Anonymous" if hidden)
- Rating (1-5 stars)
- Feedback date and preview text
- Reply status

#### Read Full Feedback
- Click on feedback to view complete message
- See customer rating and creation date
- View any existing admin replies

#### Reply to Feedback
1. Click **"Reply"** button on feedback
2. Type your **admin response** message
3. Address customer concerns or thank them for feedback
4. Click **"Post Reply"** to post your reply

#### Feedback Visibility
- Track which feedback came from **named** vs **anonymous** customers
- Filter to show only **replied** or **unreplied** feedback
- See feedback **rating distribution**

### 👥 **Account / Admin Info**
- View admin profile information
- Update admin details (name, email, etc.)
- Change admin password
- View admin privileges and permissions

### ℹ️ **About Us**
- Manage company information
- View/edit business description
- Display important business details
- Company contact information

### 🚪 **Logout**
- Click **"Logout"** button to exit admin portal
- Secure session termination

---

## 🗄️ Database Structure

### SQLite Database
- **Location**: `app.db` in the application directory
- **Type**: SQLite (file-based, no server required)
- **ORM**: Entity Framework Core 8.0

### Database Tables

#### **Users Table**
Stores user account information for authentication and profiles.

```sql
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Username TEXT NOT NULL UNIQUE,
    Email TEXT NOT NULL UNIQUE,
    ContactNumber TEXT NOT NULL,
    HouseNo TEXT NOT NULL,
    Street TEXT NOT NULL,
    Barangay TEXT NOT NULL,
    Password TEXT NOT NULL,
    Role TEXT NOT NULL DEFAULT 'Customer',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

| Column | Type | Description | Constraints |
|--------|------|-------------|-------------|
| `Id` | INTEGER | Unique user identifier | PRIMARY KEY, AUTO INCREMENT |
| `FullName` | TEXT | User's complete name | NOT NULL, Max 100 chars |
| `Username` | TEXT | Unique login identifier | NOT NULL, UNIQUE, Max 50 chars |
| `Email` | TEXT | Email address | NOT NULL, UNIQUE, Max 100 chars |
| `ContactNumber` | TEXT | Phone number | NOT NULL, Max 20 chars |
| `HouseNo` | TEXT | House/building number | NOT NULL, Max 50 chars |
| `Street` | TEXT | Street name | NOT NULL, Max 100 chars |
| `Barangay` | TEXT | Barangay location | NOT NULL, Max 50 chars |
| `Password` | TEXT | Hashed password | NOT NULL, Max 255 chars |
| `Role` | TEXT | User role | NOT NULL, Default: 'Customer' |
| `CreatedAt` | DATETIME | Account creation timestamp | NOT NULL, DEFAULT CURRENT_TIMESTAMP |

#### **Orders Table**
Records all customer orders and their status.

```sql
CREATE TABLE Orders (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT,
    CustomerName TEXT NOT NULL,
    HouseNo TEXT NOT NULL,
    Street TEXT NOT NULL,
    Barangay TEXT NOT NULL,
    City TEXT DEFAULT 'Navotas City',
    UseSavedAddress INTEGER NOT NULL,
    HasGallon INTEGER NOT NULL,
    PurchaseGallon INTEGER NOT NULL,
    RefillGallon INTEGER NOT NULL,
    Type TEXT NOT NULL,
    Quantity INTEGER NOT NULL,
    PaymentMethod TEXT NOT NULL,
    TotalPrice REAL NOT NULL,
    Status TEXT DEFAULT 'Pending',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

| Column | Type | Description | Constraints |
|--------|------|-------------|-------------|
| `Id` | INTEGER | Unique order identifier | PRIMARY KEY, AUTO INCREMENT |
| `Username` | TEXT | Associated customer | Max 50 chars |
| `CustomerName` | TEXT | Name on the order | NOT NULL, Max 100 chars |
| `HouseNo` | TEXT | Delivery house number | NOT NULL, Max 50 chars |
| `Street` | TEXT | Delivery street | NOT NULL, Max 100 chars |
| `Barangay` | TEXT | Delivery barangay | NOT NULL, Max 50 chars |
| `City` | TEXT | Delivery city | Default: 'Navotas City' |
| `UseSavedAddress` | INTEGER | Boolean flag (0/1) | NOT NULL |
| `HasGallon` | INTEGER | Does customer have gallon? (0/1) | NOT NULL |
| `PurchaseGallon` | INTEGER | Purchasing gallon? (0/1) | NOT NULL |
| `RefillGallon` | INTEGER | Refilling gallon? (0/1) | NOT NULL |
| `Type` | TEXT | Gallon type (Slim/Round) | NOT NULL, Max 10 chars |
| `Quantity` | INTEGER | Quantity ordered | NOT NULL, Range: 1-1000 |
| `PaymentMethod` | TEXT | Payment method (Cash/E-Wallet) | NOT NULL, Max 20 chars |
| `TotalPrice` | REAL | Order total amount | NOT NULL |
| `Status` | TEXT | Order fulfillment status | Default: 'Pending' |
| `CreatedAt` | DATETIME | Order creation timestamp | NOT NULL, DEFAULT CURRENT_TIMESTAMP |

#### **Feedbacks Table**
Customer feedback and admin responses.

```sql
CREATE TABLE Feedbacks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT,
    Message TEXT NOT NULL,
    Rating INTEGER NOT NULL,
    IsAnonymous INTEGER NOT NULL,
    AdminReply TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

| Column | Type | Description | Constraints |
|--------|------|-------------|-------------|
| `Id` | INTEGER | Unique feedback identifier | PRIMARY KEY, AUTO INCREMENT |
| `Username` | TEXT | Customer who submitted feedback | Max 50 chars |
| `Message` | TEXT | Feedback message | NOT NULL |
| `Rating` | INTEGER | Rating (1-5 stars) | NOT NULL, Range: 1-5 |
| `IsAnonymous` | INTEGER | Anonymous flag (0/1) | NOT NULL |
| `AdminReply` | TEXT | Admin's response | Max 1000 chars (nullable) |
| `CreatedAt` | DATETIME | Submission timestamp | NOT NULL, DEFAULT CURRENT_TIMESTAMP |

---

## 🏗️ Project Structure

```
test/
├── Components/
│   ├── _Imports.razor                  # Global component imports
│   ├── App.razor                       # Root application component
│   ├── Routes.razor                    # Application routing configuration
│   │
│   ├── Layout/
│   │   ├── MainLayout.razor            # Default layout for guest/login pages
│   │   ├── MainLayout.razor.css        # Main layout styles
│   │   ├── AdminLayout.razor           # Admin portal layout
│   │   ├── AdminLayout.razor.css       # Admin layout styles
│   │   ├── CustomerLayout.razor        # Customer portal layout
│   │   └── CustomerLayout.razor.css    # Customer layout styles
│   │
│   └── Pages/
│       ├── Error.razor                 # Error page (500, etc.)
│       ├── Login.razor                 # Login page
│       ├── Signup.razor                # Sign-up/registration page
│       ├── ForgotPassword.razor        # Password recovery page
│       ├── ViewDatabase.razor          # Database viewer (dev only)
│       │
│       ├── Admin/                      # Admin-only pages (requires admin role)
│       │   ├── Home.razor              # Admin dashboard
│       │   ├── Dashboard.razor         # Sales analytics
│       │   ├── Orders.razor            # Order management
│       │   ├── Feedback.razor          # Feedback management
│       │   ├── Account.razor           # Admin account settings
│       │   └── About.razor             # Business information
│       │
│       └── Customer/                   # Customer-only pages (requires customer role)
│           ├── Home.razor              # Customer home/welcome
│           ├── PlaceOrder.razor        # New order creation
│           ├── OrderHistory.razor      # View past orders
│           ├── Feedback.razor          # Submit & view feedback
│           ├── Account.razor           # Customer account settings
│           └── About.razor             # About the business
│
├── Data/
│   └── AppDbContext.cs                 # Entity Framework database context
│
├── Models/
│   ├── User.cs                         # User entity (Customer/Admin)
│   ├── Order.cs                        # Order entity
│   └── Feedback.cs                     # Feedback entity
│
├── Services/
│   └── CurrentUserContext.cs           # Current user state management
│
├── Properties/
│   └── launchSettings.json             # Application launch configuration
│
├── wwwroot/
│   ├── app.css                         # Global styles
│   ├── images/
│   │   └── logo.png                    # Application logo
│   └── ...                             # Static files
│
├── Program.cs                          # Application startup configuration
├── test.csproj                         # Project file
├── appsettings.json                    # Application settings
├── appsettings.Development.json        # Development-specific settings
└── app.db                              # SQLite database file
```

---

## 🔧 Core Application Files

### **Program.cs**
Application startup and dependency injection configuration:

```csharp
// Key configurations:
- Razor Components and Interactive Server Components
- SQLite database connection
- DbContext registration (AppDbContext)
- User context service (CurrentUserContext)
- Database initialization and migration
- Kestrel server configuration
```

### **AppDbContext.cs** (Data/AppDbContext.cs)
Entity Framework Core database context:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    
    // Unique constraints on Username and Email
    // Address property marked as ignored (computed property)
}
```

### **Models**

#### User.cs
- Represents a user account (Customer or Admin)
- Properties: FullName, Username, Email, Password, ContactNumber, Address, Role, CreatedAt
- Validations: Required fields, string lengths, email format

#### Order.cs
- Represents a water refill order
- Properties: OrderId, Username, CustomerName, DeliveryAddress, GallonType, Quantity, PaymentMethod, Status, TotalPrice, CreatedAt
- Supports: Purchase gallons, refill gallons, cash/e-wallet payment

#### Feedback.cs
- Represents customer feedback
- Properties: Id, Username, Message, Rating (1-5), IsAnonymous, AdminReply, CreatedAt
- Tracks: Customer satisfaction and admin responses

### **Services**

#### CurrentUserContext.cs
Simple session management service:
- Stores current logged-in user information
- Tracks user role (Customer/Admin)
- Available throughout the application session

---

## 🎨 Layout & Styling

### Layout Components

#### **MainLayout.razor**
Used for login, signup, and public-facing pages.

#### **CustomerLayout.razor**
Navigation and layout for customer dashboard:
- Customer-specific sidebar/navigation
- Links to Home, Place Order, Order History, Feedback, Account, About
- Logout option

#### **AdminLayout.razor**
Navigation and layout for admin dashboard:
- Admin-specific sidebar/navigation
- Links to Home, Orders, Feedback, Account, About
- Logout option

### Styling
- **Global Styles**: `wwwroot/app.css`
- **Layout-Specific**: `MainLayout.razor.css`, `CustomerLayout.razor.css`, `AdminLayout.razor.css`
- **Framework**: Bootstrap (via Blazor template)

---

## 🔐 Security Features

### Authentication
- Username/password-based login
- Unique username and email validation
- Password stored as text (in production, use hashing like bcrypt)

### Authorization
- Role-based access control (Customer vs. Admin)
- Page-level authorization checks
- Admin pages only accessible to admin users

### Data Validation
- Model-level validation via data annotations
- Client-side validation in forms
- Server-side validation on submit

### Best Practices
- Unique constraints on username and email
- Address validation (house, street, barangay required)
- Order quantity limits (1-1000)
- Rating validation (1-5 stars)

---

## 📊 Key Features

### 🛒 **Order Management**
- Create new orders with customizable gallon options
- Track order status in real-time
- View order history
- Multiple payment methods (Cash, E-Wallet)

### 💬 **Feedback System**
- Leave reviews with ratings
- Anonymous feedback option
- Admin reply capability
- Real-time feedback updates

### 📦 **Admin Controls**
- Centralized order management
- Payment status tracking
- Customer feedback oversight
- Sales analytics and reporting

### 👤 **User Accounts**
- Secure registration process
- Profile management
- Address book functionality
- Role-based access control

### 🌐 **Real-Time Updates**
- Blazor Server provides live UI updates
- No page refresh needed
- Instant feedback from server

---

## 🛠️ Technologies & Dependencies

### Core Framework
- **ASP.NET Core 8.0**
- **Blazor Server**
- **Entity Framework Core 8.0**

### Database
- **SQLite** (lightweight, file-based)

### Frontend
- **Razor Components**
- **HTML5 / CSS3**
- **Bootstrap** (included in Blazor template)

### Additional Libraries
- **Microsoft.EntityFrameworkCore.Sqlite**
- **System.ComponentModel.DataAnnotations** (validation)

---

## 🚨 Troubleshooting

### Port Already in Use Error
If you see: "Failed to bind to address http://127.0.0.1:5295: address already in use"

**Solution**:
```bash
# Option 1: Change the port in launchSettings.json
# Edit Properties/launchSettings.json and change "https" port to 5296 or 5297

# Option 2: Kill the process using the port
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

# This will recreate the database with proper schema
```

### Login Issues
- Ensure you created an account via the **Sign-Up** page
- Check that your **username** and **password** match exactly (case-sensitive)
- Verify you selected the correct role (**Customer** or **Admin**)

### Order Placement Issues
- Ensure all required fields are filled
- Check that quantity is between 1 and 1000
- Verify delivery address is complete
- Confirm payment method is selected

---

## 📚 Learning Objectives

This project is designed to teach:

1. **Web Development**:
   - Building interactive web applications with Blazor
   - Real-time updates without page refresh
   - Server-side rendering with .NET

2. **Object-Oriented Programming**:
   - Entity models (User, Order, Feedback classes)
   - Encapsulation and abstraction
   - Inheritance and polymorphism

3. **Database Design**:
   - Relational database structure
   - Entity relationships
   - Data integrity and constraints
   - ACID principles

4. **Back-End Development**:
   - Entity Framework Core ORM
   - CRUD operations (Create, Read, Update, Delete)
   - Repository pattern (if implemented)
   - Dependency injection

5. **Software Engineering**:
   - Project structure and organization
   - Code reusability
   - Error handling and logging
   - User experience design
   - Security best practices

---

## 🎓 Best Practices Demonstrated

- ✅ Separation of concerns (Models, Data, Services, Components)
- ✅ DI (Dependency Injection) container
- ✅ Model validation using data annotations
- ✅ Role-based authorization
- ✅ Error handling with try-catch blocks
- ✅ User-friendly error messages
- ✅ Real-time UI updates
- ✅ Clean, readable code structure

---

## 📝 Development Notes

### Adding New Features
1. Create entity model in `Models/` folder
2. Add DbSet to `AppDbContext.cs`
3. Create Razor component in `Components/Pages/`
4. Update routes in `Routes.razor`
5. Test thoroughly before deployment

### Database Migrations (When Needed)
```bash
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

---

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
dotnet test.dll
```

---

## 📞 Support & Resources

### Documentation
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Blazor Server Documentation](https://docs.microsoft.com/aspnet/core/blazor)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [SQLite Documentation](https://www.sqlite.org/docs.html)

### Common Issues
- Check the **Troubleshooting** section above
- Review application logs
- Verify all prerequisites are installed
- Ensure .NET SDK 8.0 or higher is installed

---

## 📄 License

This project is for educational purposes. Use and modify as needed for learning.

---

## ✨ Future Enhancements

Potential features to add:
- 🔒 Password hashing with bcrypt
- 📧 Email notifications for orders
- 🗳️ Advanced filtering and reporting
- 💳 Integrated payment gateway (PayMaya, GCash)
- 📱 Mobile-responsive design improvements
- 🔔 Real-time notifications
- 📊 Advanced analytics dashboard
- 🌐 Multi-language support
- 🗺️ Map integration for delivery tracking
- 🤖 AI-powered recommendations

---

## 🤝 Contributing

Feel free to fork, modify, and improve this project for educational purposes!

---

## 📅 Project Information

- **Framework**: ASP.NET Core 8.0 Blazor Server
- **Database**: SQLite
- **Language**: C# with Razor
- **Purpose**: Educational - Learning C# web development

---

<p align="center">
  <strong>Thank you for using WSOA!</strong><br>
  Built with ❤️ for learning and development
</p>

---
## 🚨 Important Links
1. Figma - [Figma](https://www.figma.com/design/qDKawO6WHZ6pZayIyJRWcE/Wireframing-WSOA?node-id=0-1&t=2q8IbWdNnUfIcZg5-1)
2. UML - [Code](/test/UML.puml) OR [Image](/test/wwwroot/images/UML.png)
3. Logo - [Image](/test/wwwroot/images/logo.png)
---

**Last Updated**: November 2025  
**Version**: 1.0.0
