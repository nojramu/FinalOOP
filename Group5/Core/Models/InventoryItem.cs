namespace Group5.Models
{
    /// <summary>
    /// Represents an item in the toolroom inventory system.
    /// Tracks available quantities and enforces borrowing limits.
    /// </summary>
    public class InventoryItem
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Department/Toolroom this item belongs to
        /// </summary>
        public string Department { get; set; } = string.Empty;
        
        /// <summary>
        /// Name of the item
        /// </summary>
        public string ItemName { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the item
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Total quantity in stock (including borrowed items)
        /// </summary>
        public int TotalQuantity { get; set; }
        
        /// <summary>
        /// Currently available quantity (not borrowed)
        /// </summary>
        public int AvailableQuantity { get; set; }
        
        /// <summary>
        /// Maximum quantity a single student can borrow
        /// </summary>
        public int MaxPerStudent { get; set; }
        
        /// <summary>
        /// Whether this item is currently active/available for borrowing
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// When this item was added to inventory
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Last time this item's inventory was updated
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}

