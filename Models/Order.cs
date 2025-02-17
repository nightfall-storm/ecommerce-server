using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace server.Models;

public class Order
{
    [Key]
    public int ID { get; set; }

    [Required]
    public int ClientID { get; set; }

    [Required]
    public DateTime DateCommande { get; set; }

    [Required]
    public string Statut { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    // Navigation properties with JSON ignore to prevent circular references
    [JsonIgnore]
    public Client? Client { get; set; }
    [JsonIgnore]
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}