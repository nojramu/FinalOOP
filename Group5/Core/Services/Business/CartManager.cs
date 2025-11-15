using Group5.Data;
using Group5.Models;
using Group5.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Group5.Services.Business
{
    /// <summary>
    /// Cart Manager - Business Logic for Cart Operations
    /// Demonstrates SINGLE RESPONSIBILITY PRINCIPLE (SRP)
    /// Demonstrates ASSOCIATION - uses IInventoryService
    /// Demonstrates ENCAPSULATION - hides cart logic complexity
    /// </summary>
    public class CartManager
    {
        private readonly AppDbContext _dbContext;
        private readonly IInventoryService _inventoryService;

        /// <summary>
        /// Constructor - Demonstrates DEPENDENCY INJECTION
        /// UML: ASSOCIATION (uses-a relationship) with both dependencies
        /// </summary>
        public CartManager(AppDbContext dbContext, IInventoryService inventoryService)
        {
            _dbContext = dbContext;
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Get cart items for a user with inventory details
        /// Returns tuple with cart items and their inventory limits
        /// </summary>
        public async Task<(List<CartItem> Items, List<InventoryItem> Limits)> GetCartWithLimitsAsync(string userEmail)
        {
            var cartItems = await _dbContext.CartItems
                .Where(item => item.UserEmail == userEmail)
                .ToListAsync();

            var itemLimits = new List<InventoryItem>();
            
            if (cartItems.Any())
            {
                var departments = cartItems.Select(c => c.Department).Distinct().ToList();
                
                foreach (var dept in departments)
                {
                    var deptItems = await _inventoryService.GetDepartmentInventoryAsync(dept);
                    itemLimits.AddRange(deptItems);
                }
            }

            return (cartItems, itemLimits);
        }

        /// <summary>
        /// Clear all cart items for a user
        /// </summary>
        public async Task<bool> ClearCartAsync(string userEmail)
        {
            try
            {
                var items = await _dbContext.CartItems
                    .Where(c => c.UserEmail == userEmail)
                    .ToListAsync();
                
                _dbContext.CartItems.RemoveRange(items);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get cart summary (total items, total types)
        /// </summary>
        public async Task<(int TotalItems, int TotalTypes)> GetCartSummaryAsync(string userEmail)
        {
            var items = await _dbContext.CartItems
                .Where(c => c.UserEmail == userEmail)
                .ToListAsync();
            
            return (items.Sum(i => i.Quantity), items.Count);
        }

        /// <summary>
        /// Validate cart against current inventory
        /// Returns list of validation errors
        /// </summary>
        public async Task<List<string>> ValidateCartAsync(string userEmail)
        {
            var errors = new List<string>();
            var cartItems = await _dbContext.CartItems
                .Where(c => c.UserEmail == userEmail)
                .ToListAsync();

            foreach (var cartItem in cartItems)
            {
                var inventoryItem = await _dbContext.InventoryItems
                    .FirstOrDefaultAsync(i => i.Department == cartItem.Department && 
                                            i.ItemName == cartItem.ItemName);

                if (inventoryItem == null)
                {
                    errors.Add($"Item '{cartItem.ItemName}' no longer exists in inventory");
                    continue;
                }

                if (cartItem.Quantity > inventoryItem.AvailableQuantity)
                {
                    errors.Add($"Only {inventoryItem.AvailableQuantity} of '{cartItem.ItemName}' available (you have {cartItem.Quantity} in cart)");
                }

                if (cartItem.Quantity > inventoryItem.MaxPerStudent)
                {
                    errors.Add($"Max {inventoryItem.MaxPerStudent} per student for '{cartItem.ItemName}' (you have {cartItem.Quantity} in cart)");
                }
            }

            return errors;
        }
    }
}

