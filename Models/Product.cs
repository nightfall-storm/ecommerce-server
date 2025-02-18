using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace server.Models;

public class Product
{
    [Key]
    public int ID { get; set; }

    [Required]
    public string Nom { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Prix { get; set; }

    [Required]
    public int Stock { get; set; }

    public string ImageURL { get; set; } = string.Empty;

    public int CategorieID { get; set; }

    [JsonIgnore]
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}