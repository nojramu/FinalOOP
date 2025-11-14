using System.ComponentModel.DataAnnotations;

namespace test.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ContactNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string HouseNo { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Barangay { get; set; } = string.Empty;

    // Combined address for display purposes
    public string Address => $"{HouseNo}, {Street}, {Barangay}, Navotas City";

    [Required]
    [StringLength(255)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = "Customer"; // Default role is Customer

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

