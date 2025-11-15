using Group5.Models;
using Microsoft.EntityFrameworkCore;

namespace Group5.Data.Repositories
{
    /// <summary>
    /// Inventory Repository Implementation
    /// Demonstrates INHERITANCE - extends Repository<InventoryItem> (IS-A relationship)
    /// Demonstrates POLYMORPHISM - implements IInventoryRepository interface
    /// UML: GENERALIZATION from Repository<T>
    /// UML: REALIZATION of IInventoryRepository
    /// </summary>
    public class InventoryRepository : Repository<InventoryItem>, IInventoryRepository
    {
        /// <summary>
        /// Constructor - passes context to base class
        /// Demonstrates calling base constructor
        /// </summary>
        public InventoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<InventoryItem>> GetByDepartmentAsync(string department)
        {
            return await _dbSet
                .Where(i => i.Department == department && i.IsActive)
                .OrderBy(i => i.ItemName)
                .ToListAsync();
        }

        public async Task<int> GetAvailableQuantityAsync(string department, string itemName)
        {
            var item = await _dbSet
                .FirstOrDefaultAsync(i => i.Department == department &&
                                         i.ItemName == itemName &&
                                         i.IsActive);
            return item?.AvailableQuantity ?? 0;
        }

        public async Task<bool> UpdateAvailableQuantityAsync(string department, string itemName, int quantityChange)
        {
            var item = await _dbSet
                .FirstOrDefaultAsync(i => i.Department == department &&
                                         i.ItemName == itemName &&
                                         i.IsActive);
            
            if (item == null)
                return false;

            item.AvailableQuantity += quantityChange;
            item.LastUpdated = DateTime.Now;
            
            // Ensure we don't go below 0 or above total
            if (item.AvailableQuantity < 0)
                item.AvailableQuantity = 0;
            if (item.AvailableQuantity > item.TotalQuantity)
                item.AvailableQuantity = item.TotalQuantity;

            await SaveChangesAsync();
            return true;
        }
    }
}

