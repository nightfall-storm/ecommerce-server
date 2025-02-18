using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace server.Models;

public class OrderDetail
{
    [Key]
    public int ID { get; set; }

    [Required]
    public int CommandeID { get; set; }

    [Required]
    public int ProduitID { get; set; }

    [Required]
    public int Quantite { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrixUnitaire { get; set; }

    // Navigation properties
    [JsonIgnore]
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}