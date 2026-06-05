using System.ComponentModel.DataAnnotations;

namespace AgroSmart.Api.Dtos;

public class UserRegisterDto
{
    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = null!;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = null!;

    [MaxLength(30)]
    public string Role { get; set; } = "Operator";
}

public class UserLoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}

public class AuthResponseDto
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}
