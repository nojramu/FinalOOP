using System.ComponentModel.DataAnnotations;

namespace test.Models;

public class Order
{
    public int Id { get; set; }

    [StringLength(50)]
    public string? Username { get; set; }

    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string HouseNo { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Barangay { get; set; } = string.Empty;

    [StringLength(50)]
    public string City { get; set; } = "Navotas City";

    public bool UseSavedAddress { get; set; }

    public bool HasGallon { get; set; }

    public bool PurchaseGallon { get; set; }

    public bool RefillGallon { get; set; }

    [Required]
    [StringLength(10)]
    public string Type { get; set; } = string.Empty; // Slim or Round

    [Required]
    [Range(1, 1000)]
    public int Quantity { get; set; }

    [Required]
    [StringLength(20)]
    public string PaymentMethod { get; set; } = string.Empty; // cash / e-wallet

    [Range(0, 1000000)]
    public decimal TotalPrice { get; set; }

    [StringLength(30)]
    public string? PaymentStatus { get; set; }

    [StringLength(30)]
    public string? DeliveryStatus { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? TimeDelivered { get; set; }
}
