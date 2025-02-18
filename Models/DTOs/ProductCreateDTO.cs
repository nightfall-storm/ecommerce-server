namespace server.Models.DTOs;

public class ProductCreateDTO
{
    public string Nom { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Prix { get; set; }
    public int Stock { get; set; }
    public IFormFile? Image { get; set; }
    public int CategorieID { get; set; }
}