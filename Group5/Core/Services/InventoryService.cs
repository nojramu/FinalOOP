using Group5.Data;
using Group5.Models;
using Group5.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Group5.Services
{
    /// <summary>
    /// Service for managing inventory and cart operations with proper validation
    /// Implements IInventoryService interface - Demonstrates POLYMORPHISM
    /// Uses AppDbContext - Demonstrates ASSOCIATION (uses-a relationship)
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly AppDbContext _dbContext;

        public InventoryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Add item to cart with validation against MaxPerStudent limit
        /// </summary>
        public async Task<(bool Success, string Message)> AddToCartAsync(
            string userEmail,
            string department,
            string itemName,
            int quantityToAdd)
        {
            try
            {
                // Get inventory item
                var inventoryItem = await _dbContext.InventoryItems
                    .FirstOrDefaultAsync(i => i.Department == department &&
                                             i.ItemName == itemName &&
                                             i.IsActive);

                if (inventoryItem == null)
                {
                    return (false, "Item not found in inventory.");
                }

                // Check if enough items are available
                if (inventoryItem.AvailableQuantity < quantityToAdd)
                {
                    return (false, $"Only {inventoryItem.AvailableQuantity} items available.");
                }

                // Check existing cart items for this user
                var existingCartItem = await _dbContext.CartItems
                    .FirstOrDefaultAsync(c => c.UserEmail == userEmail &&
                                             c.ItemName == itemName &&
                                             c.Department == department);

                int currentCartQuantity = existingCartItem?.Quantity ?? 0;
                int newTotalQuantity = currentCartQuantity + quantityToAdd;

                // Validate against MaxPerStudent limit
                if (newTotalQuantity > inventoryItem.MaxPerStudent)
                {
                    int remainingAllowed = inventoryItem.MaxPerStudent - currentCartQuantity;
                    if (remainingAllowed <= 0)
                    {
                        return (false, $"You've already reached the maximum limit of {inventoryItem.MaxPerStudent} for this item.");
                    }
                    return (false, $"You can only add {remainingAllowed} more. Maximum allowed per student: {inventoryItem.MaxPerStudent}");
                }

                // Add or update cart item
                if (existingCartItem != null)
                {
                    existingCartItem.Quantity = newTotalQuantity;
                }
                else
                {
                    var newCartItem = new CartItem
                    {
                        UserEmail = userEmail,
                        Department = department,
                        ItemName = itemName,
                        Quantity = quantityToAdd
                    };
                    _dbContext.CartItems.Add(newCartItem);
                }

                await _dbContext.SaveChangesAsync();
                return (true, $"✅ Added {quantityToAdd} × {itemName} to cart!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error adding to cart: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update cart item quantity with validation
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateCartQuantityAsync(
            int cartItemId,
            string userEmail,
            int newQuantity)
        {
            try
            {
                var cartItem = await _dbContext.CartItems
                    .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserEmail == userEmail);

                if (cartItem == null)
                {
                    return (false, "Cart item not found.");
                }

                if (newQuantity <= 0)
                {
                    // Remove item if quantity is 0 or less
                    _dbContext.CartItems.Remove(cartItem);
                    await _dbContext.SaveChangesAsync();
                    return (true, "Item removed from cart.");
                }

                // Get inventory item for validation
                var inventoryItem = await _dbContext.InventoryItems
                    .FirstOrDefaultAsync(i => i.Department == cartItem.Department &&
                                             i.ItemName == cartItem.ItemName &&
                                             i.IsActive);

                if (inventoryItem == null)
                {
                    return (false, "Item no longer available in inventory.");
                }

                // Validate against MaxPerStudent
                if (newQuantity > inventoryItem.MaxPerStudent)
                {
                    return (false, $"Maximum allowed per student: {inventoryItem.MaxPerStudent}");
                }

                // Validate against available quantity
                if (newQuantity > inventoryItem.AvailableQuantity)
                {
                    return (false, $"Only {inventoryItem.AvailableQuantity} items available.");
                }

                cartItem.Quantity = newQuantity;
                await _dbContext.SaveChangesAsync();
                return (true, "Cart updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error updating cart: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Decrease inventory when items are issued
        /// </summary>
        public async Task<(bool Success, string Message)> IssueItemsAsync(List<BorrowFormItem> items)
        {
            try
            {
                foreach (var item in items)
                {
                    var inventoryItem = await _dbContext.InventoryItems
                        .FirstOrDefaultAsync(i => i.Department == item.Department &&
                                                 i.ItemName == item.ItemName &&
                                                 i.IsActive);

                    if (inventoryItem == null)
                    {
                        return (false, $"Item '{item.ItemName}' not found in inventory.");
                    }

                    if (inventoryItem.AvailableQuantity < item.Quantity)
                    {
                        return (false, $"Insufficient quantity for '{item.ItemName}'. Available: {inventoryItem.AvailableQuantity}, Requested: {item.Quantity}");
                    }

                    // Decrease available quantity
                    inventoryItem.AvailableQuantity -= item.Quantity;
                    inventoryItem.LastUpdated = DateTime.Now;
                }

                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"✅ Inventory updated: {items.Count} item types issued.");
                return (true, "Inventory updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error issuing items: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Increase inventory when items are returned
        /// </summary>
        public async Task<(bool Success, string Message)> ReturnItemsAsync(List<BorrowFormItem> items)
        {
            try
            {
                foreach (var item in items)
                {
                    var inventoryItem = await _dbContext.InventoryItems
                        .FirstOrDefaultAsync(i => i.Department == item.Department &&
                                                 i.ItemName == item.ItemName &&
                                                 i.IsActive);

                    if (inventoryItem == null)
                    {
                        return (false, $"Item '{item.ItemName}' not found in inventory.");
                    }

                    // Increase available quantity
                    inventoryItem.AvailableQuantity += item.Quantity;
                    
                    // Ensure we don't exceed total quantity
                    if (inventoryItem.AvailableQuantity > inventoryItem.TotalQuantity)
                    {
                        inventoryItem.AvailableQuantity = inventoryItem.TotalQuantity;
                    }
                    
                    inventoryItem.LastUpdated = DateTime.Now;
                }

                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"✅ Inventory updated: {items.Count} item types returned.");
                return (true, "Inventory updated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error returning items: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get inventory items for a specific department
        /// </summary>
        public async Task<List<InventoryItem>> GetDepartmentInventoryAsync(string department)
        {
            return await _dbContext.InventoryItems
                .Where(i => i.Department == department && i.IsActive)
                .OrderBy(i => i.ItemName)
                .ToListAsync();
        }

        /// <summary>
        /// Get available quantity for a specific item (real-time)
        /// </summary>
        public async Task<int> GetAvailableQuantityAsync(string department, string itemName)
        {
            var item = await _dbContext.InventoryItems
                .FirstOrDefaultAsync(i => i.Department == department &&
                                         i.ItemName == itemName &&
                                         i.IsActive);
            return item?.AvailableQuantity ?? 0;
        }
    }
}

