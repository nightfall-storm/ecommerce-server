namespace server.Models.DTOs;

public class ClientDTO
{
    public int ID { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public ICollection<OrderDTO> Orders { get; set; } = new List<OrderDTO>();
}

public class OrderDTO
{
    public int ID { get; set; }
    public int ClientID { get; set; }
    public DateTime DateCommande { get; set; }
    public string Statut { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public ICollection<OrderDetailDTO> OrderDetails { get; set; } = new List<OrderDetailDTO>();
}