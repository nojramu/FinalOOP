namespace Group5.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string Department { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string UserEmail { get; set; } = string.Empty;
    }
}

