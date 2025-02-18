namespace server.Models.DTOs;

public class ClientPatchDTO
{
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? Email { get; set; }
    public string? Adresse { get; set; }
    public string? Telephone { get; set; }
}

public class ProductPatchDTO
{
    public string? Nom { get; set; }
    public string? Description { get; set; }
    public decimal? Prix { get; set; }
    public int? Stock { get; set; }
    public IFormFile? Image { get; set; }
    public int? CategorieID { get; set; }
}

public class OrderPatchDTO
{
    public string? Statut { get; set; }
    public decimal? Total { get; set; }
}

public class OrderDetailPatchDTO
{
    public int? Quantite { get; set; }
    public decimal? PrixUnitaire { get; set; }
}