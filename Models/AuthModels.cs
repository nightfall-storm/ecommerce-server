using System.ComponentModel.DataAnnotations;

namespace server.Models;

public class RegisterRequest
{
    [Required]
    public string Nom { get; set; } = string.Empty;

    [Required]
    public string Prenom { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string MotDePasse { get; set; } = string.Empty;

    public string? Adresse { get; set; }

    public string? Telephone { get; set; }

    public string Role { get; set; } = "user"; // Default role
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string MotDePasse { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public Client User { get; set; } = null!;
}