namespace Group5.Models
{
    public class BorrowedItem
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int OfficialBorrowListRecordId { get; set; }
        public OfficialBorrowListRecord? OfficialBorrowListRecord { get; set; }
    }
}

