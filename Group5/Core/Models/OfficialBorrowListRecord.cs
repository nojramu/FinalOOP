namespace Group5.Models
{
    public class OfficialBorrowListRecord
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public string ReferenceCode { get; set; } = string.Empty;
        public string ProfessorInCharge { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
        public string UserEmail { get; set; } = string.Empty;
        public List<BorrowedItem> BorrowedItems { get; set; } = new();
    }
}

