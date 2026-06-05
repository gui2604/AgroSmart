using System.ComponentModel.DataAnnotations;

namespace AgroSmart.Api.Models;

/// <summary>An authenticated operator/administrator of the platform.</summary>
public class User
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(300)]
    public string PasswordHash { get; set; } = null!;

    [Required]
    [MaxLength(30)]
    public string Role { get; set; } = "Operator";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Alert> AcknowledgedAlerts { get; set; } = new List<Alert>();
}
