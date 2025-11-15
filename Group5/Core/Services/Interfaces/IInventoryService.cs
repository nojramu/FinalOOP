using Group5.Models;

namespace Group5.Services.Interfaces
{
    /// <summary>
    /// Interface for Inventory Service - Demonstrates ABSTRACTION principle
    /// Defines contract for inventory management operations
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Add item to cart with validation against MaxPerStudent limit
        /// </summary>
        Task<(bool Success, string Message)> AddToCartAsync(
            string userEmail,
            string department,
            string itemName,
            int quantityToAdd);

        /// <summary>
        /// Update cart item quantity with validation
        /// </summary>
        Task<(bool Success, string Message)> UpdateCartQuantityAsync(
            int cartItemId,
            string userEmail,
            int newQuantity);

        /// <summary>
        /// Decrease inventory when items are issued
        /// </summary>
        Task<(bool Success, string Message)> IssueItemsAsync(List<BorrowFormItem> items);

        /// <summary>
        /// Increase inventory when items are returned
        /// </summary>
        Task<(bool Success, string Message)> ReturnItemsAsync(List<BorrowFormItem> items);

        /// <summary>
        /// Get inventory items for a specific department
        /// </summary>
        Task<List<InventoryItem>> GetDepartmentInventoryAsync(string department);

        /// <summary>
        /// Get available quantity for a specific item (real-time)
        /// </summary>
        Task<int> GetAvailableQuantityAsync(string department, string itemName);
    }
}

