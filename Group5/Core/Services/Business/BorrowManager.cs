using Group5.Data;
using Group5.Models;
using Microsoft.EntityFrameworkCore;

namespace Group5.Services.Business
{
    /// <summary>
    /// Borrow Manager - Business Logic for Borrow Operations
    /// Demonstrates SINGLE RESPONSIBILITY PRINCIPLE (SRP)
    /// Demonstrates ASSOCIATION - uses AppDbContext
    /// Demonstrates ENCAPSULATION - hides borrow logic complexity
    /// </summary>
    public class BorrowManager
    {
        private readonly AppDbContext _dbContext;

        /// <summary>
        /// Constructor - Demonstrates DEPENDENCY INJECTION
        /// UML: ASSOCIATION (uses-a relationship) with DbContext
        /// </summary>
        public BorrowManager(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Create borrow form from cart items
        /// Returns the created form's reference code
        /// Note: This method uses the existing BorrowForm model properties
        /// </summary>
        public async Task<(bool Success, string ReferenceCode, string Message)> CreateBorrowFormFromCartAsync(
            string studentEmail, 
            string studentName, 
            string studentNumber, 
            string professorEmail,
            string subjectCode)
        {
            try
            {
                // Get cart items
                var cartItems = await _dbContext.CartItems
                    .Where(c => c.UserEmail == studentEmail)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    return (false, string.Empty, "Cart is empty");
                }

                // Generate unique reference code
                var referenceCode = $"BRW-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

                // Create borrow form
                var borrowForm = new BorrowForm
                {
                    ReferenceCode = referenceCode,
                    StudentEmail = studentEmail,
                    StudentName = studentName,
                    StudentNumber = studentNumber,
                    ProfessorEmail = professorEmail,
                    SubjectCode = subjectCode,
                    SubmittedAt = DateTime.Now,
                    IsApproved = null, // Pending
                    ProcessedAt = null,
                    ProcessedBy = string.Empty,
                    RejectionReason = string.Empty,
                    IsIssued = false,
                    IsReturned = false,
                    Items = new List<BorrowFormItem>()
                };

                // Convert cart items to borrow form items
                foreach (var cartItem in cartItems)
                {
                    borrowForm.Items.Add(new BorrowFormItem
                    {
                        Department = cartItem.Department,
                        ItemName = cartItem.ItemName,
                        Quantity = cartItem.Quantity
                    });
                }

                // Save to database
                _dbContext.BorrowForms.Add(borrowForm);
                await _dbContext.SaveChangesAsync();

                // Clear cart
                _dbContext.CartItems.RemoveRange(cartItems);
                await _dbContext.SaveChangesAsync();

                return (true, referenceCode, "Borrow form created successfully");
            }
            catch (Exception ex)
            {
                return (false, string.Empty, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get pending forms (not yet approved/rejected)
        /// </summary>
        public async Task<List<BorrowForm>> GetPendingFormsAsync()
        {
            return await _dbContext.BorrowForms
                .Include(f => f.Items)
                .Where(f => f.IsApproved == null)
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get approved forms that haven't been issued yet
        /// </summary>
        public async Task<List<BorrowForm>> GetApprovedNotIssuedAsync()
        {
            return await _dbContext.BorrowForms
                .Include(f => f.Items)
                .Where(f => f.IsApproved == true && !f.IsIssued && !f.IsReturned)
                .OrderByDescending(f => f.ProcessedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get issued forms that haven't been returned yet
        /// </summary>
        public async Task<List<BorrowForm>> GetIssuedNotReturnedAsync()
        {
            return await _dbContext.BorrowForms
                .Include(f => f.Items)
                .Where(f => f.IsIssued && !f.IsReturned)
                .OrderByDescending(f => f.ProcessedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Approve a borrow form
        /// </summary>
        public async Task<(bool Success, string Message)> ApproveFormAsync(int formId)
        {
            try
            {
                var form = await _dbContext.BorrowForms
                    .Include(f => f.Items)
                    .FirstOrDefaultAsync(f => f.Id == formId);

                if (form == null)
                    return (false, "Form not found");

                if (form.IsApproved != null)
                    return (false, "Form already processed");

                form.IsApproved = true;
                form.ProcessedAt = DateTime.Now;

                await _dbContext.SaveChangesAsync();
                return (true, $"Form {form.ReferenceCode} approved");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Reject a borrow form
        /// </summary>
        public async Task<(bool Success, string Message)> RejectFormAsync(int formId)
        {
            try
            {
                var form = await _dbContext.BorrowForms
                    .FirstOrDefaultAsync(f => f.Id == formId);

                if (form == null)
                    return (false, "Form not found");

                if (form.IsApproved != null)
                    return (false, "Form already processed");

                form.IsApproved = false;
                form.ProcessedAt = DateTime.Now;

                await _dbContext.SaveChangesAsync();
                return (true, $"Form {form.ReferenceCode} rejected");
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}

