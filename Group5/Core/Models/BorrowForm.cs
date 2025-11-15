namespace Group5.Models
{
    public class BorrowForm
    {
        public int Id { get; set; }
        public string ReferenceCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string ProfessorEmail { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public List<BorrowFormItem> Items { get; set; } = new();
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public bool? IsApproved { get; set; } = null;
        public DateTime? ProcessedAt { get; set; } = null;
        public string ProcessedBy { get; set; } = string.Empty;
        public string RejectionReason { get; set; } = string.Empty;
        public bool IsIssued { get; set; } = false;
        public bool IsReturned { get; set; } = false;
    }

    public class BorrowFormItem
    {
        public int Id { get; set; }
        public string Department { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int BorrowFormId { get; set; }
        public BorrowForm? BorrowForm { get; set; }
    }
}

