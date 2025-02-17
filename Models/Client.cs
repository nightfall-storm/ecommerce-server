using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace server.Models;

public class Client
{
    [Key]
    public int ID { get; set; }

    [Required]
    public string Nom { get; set; } = string.Empty;

    [Required]
    public string Prenom { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string MotDePasse { get; set; } = string.Empty;

    public string Adresse { get; set; } = string.Empty;

    public string Telephone { get; set; } = string.Empty;

    // Navigation property with JSON ignore to prevent circular references
    [JsonIgnore]
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}