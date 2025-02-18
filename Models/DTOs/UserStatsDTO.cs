namespace server.Models.DTOs;

public class UserStatsDTO
{
    public int ID { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    // Order Statistics
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public int TotalProductsBought { get; set; }
    public Dictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();
    public DateTime? LastOrderDate { get; set; }
    public List<RecentOrderDTO> RecentOrders { get; set; } = new List<RecentOrderDTO>();
}

public class RecentOrderDTO
{
    public int ID { get; set; }
    public DateTime DateCommande { get; set; }
    public string Statut { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int NumberOfItems { get; set; }
}