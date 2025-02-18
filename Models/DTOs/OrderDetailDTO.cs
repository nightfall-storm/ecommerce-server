namespace server.Models.DTOs;

public class OrderDetailDTO
{
    public int ID { get; set; }
    public int CommandeID { get; set; }
    public int ProduitID { get; set; }
    public int Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public ProductDTO? Product { get; set; }
}

public class ProductDTO
{
    public int ID { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Prix { get; set; }
    public int Stock { get; set; }
    public string ImageURL { get; set; } = string.Empty;
    public int CategorieID { get; set; }
}