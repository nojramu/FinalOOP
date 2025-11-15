using Group5.Models;

namespace Group5.Data.Repositories
{
    /// <summary>
    /// Inventory-specific Repository Interface
    /// Demonstrates INHERITANCE - extends IRepository<InventoryItem>
    /// UML: GENERALIZATION (IS-A relationship)
    /// </summary>
    public interface IInventoryRepository : IRepository<InventoryItem>
    {
        /// <summary>
        /// Get inventory items for specific department
        /// </summary>
        Task<List<InventoryItem>> GetByDepartmentAsync(string department);

        /// <summary>
        /// Get available quantity for specific item
        /// </summary>
        Task<int> GetAvailableQuantityAsync(string department, string itemName);

        /// <summary>
        /// Update available quantity
        /// </summary>
        Task<bool> UpdateAvailableQuantityAsync(string department, string itemName, int quantityChange);
    }
}

