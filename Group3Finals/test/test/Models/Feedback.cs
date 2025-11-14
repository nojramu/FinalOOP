using System.ComponentModel.DataAnnotations;

namespace test.Models;

public class Feedback
{
    public int Id { get; set; }

    [StringLength(50)]
    public string? Username { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    public bool IsAnonymous { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? AdminReply { get; set; }

    public DateTime? AdminReplyDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Display name based on anonymous setting
    public string DisplayName
    {
        get
        {
            if (!IsAnonymous || string.IsNullOrWhiteSpace(FullName))
                return FullName;

            if (FullName.Length <= 2)
                return FullName[0] + "***";

            if (FullName.Length <= 3)
                return FullName[0] + "**" + FullName[FullName.Length - 1];

            // For names 4+ characters: first char + asterisks + last 2 chars
            // Example: "Angela" (6) → "A***la"
            var asteriskCount = FullName.Length - 3;
            return FullName[0] + new string('*', asteriskCount) + FullName.Substring(FullName.Length - 2);
        }
    }
}

