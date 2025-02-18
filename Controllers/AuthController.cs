using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using server.Data;
using server.Models;
using server.Services;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using server.Models.DTOs;

namespace server.Controllers;

/// <summary>
/// Controller for handling authentication operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Authentication")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApplicationDbContext context, AuthService authService, ILogger<AuthController> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">User registration details</param>
    /// <returns>Authentication token and user details</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        // Check if email already exists
        if (await _context.Clients.AnyAsync(c => c.Email == request.Email))
        {
            return BadRequest("Email already registered");
        }

        // Validate role
        if (!string.IsNullOrEmpty(request.Role) && request.Role != "user" && request.Role != "admin")
        {
            return BadRequest("Invalid role. Role must be either 'user' or 'admin'");
        }

        // Create new client
        var client = new Client
        {
            Nom = request.Nom,
            Prenom = request.Prenom,
            Email = request.Email,
            MotDePasse = _authService.HashPassword(request.MotDePasse),
            Adresse = request.Adresse ?? string.Empty,
            Telephone = request.Telephone ?? string.Empty,
            Role = request.Role ?? "user" // Set default role to "user" if not specified
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        // Generate JWT token
        var token = _authService.GenerateJwtToken(client);

        return new AuthResponse
        {
            Token = token,
            User = client
        };
    }

    /// <summary>
    /// Login with existing credentials
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication token and user details</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == request.Email);

        if (client == null)
        {
            return BadRequest("Invalid email or password");
        }

        if (!_authService.VerifyPassword(request.MotDePasse, client.MotDePasse))
        {
            return BadRequest("Invalid email or password");
        }

        // Generate JWT token
        var token = _authService.GenerateJwtToken(client);

        return new AuthResponse
        {
            Token = token,
            User = client
        };
    }

    /// <summary>
    /// Get current authenticated user's information with order statistics
    /// </summary>
    /// <returns>User details and order statistics</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserStatsDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserStatsDTO>> GetCurrentUser()
    {
        try
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("User is not authenticated");
                return Unauthorized("User is not authenticated");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null || !int.TryParse(userId, out int userIdInt))
            {
                _logger.LogWarning("Invalid user ID in token");
                return BadRequest("Invalid user ID format");
            }

            var client = await _context.Clients
                .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderDetails)
                .FirstOrDefaultAsync(c => c.ID == userIdInt);

            if (client == null)
            {
                _logger.LogWarning("No client found with ID: {UserId}", userIdInt);
                return NotFound($"No user found with ID: {userIdInt}");
            }

            // Calculate order statistics
            var orders = client.Orders.ToList();
            var orderDetails = orders.SelectMany(o => o.OrderDetails).ToList();

            var userStats = new UserStatsDTO
            {
                ID = client.ID,
                Nom = client.Nom,
                Prenom = client.Prenom,
                Email = client.Email,
                Adresse = client.Adresse,
                Telephone = client.Telephone,
                Role = client.Role,

                // Order statistics
                TotalOrders = orders.Count,
                TotalSpent = orders.Sum(o => o.Total),
                TotalProductsBought = orderDetails.Sum(od => od.Quantite),
                OrdersByStatus = orders.GroupBy(o => o.Statut)
                                     .ToDictionary(g => g.Key, g => g.Count()),
                LastOrderDate = orders.Any() ?
                    orders.Max(o => o.DateCommande) : null,

                // Get 5 most recent orders
                RecentOrders = orders
                    .OrderByDescending(o => o.DateCommande)
                    .Take(5)
                    .Select(o => new RecentOrderDTO
                    {
                        ID = o.ID,
                        DateCommande = o.DateCommande,
                        Statut = o.Statut,
                        Total = o.Total,
                        NumberOfItems = o.OrderDetails.Sum(od => od.Quantite)
                    })
                    .ToList()
            };

            _logger.LogInformation("Successfully retrieved user stats for: {UserId}", userIdInt);
            return userStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCurrentUser: {Message}", ex.Message);
            return StatusCode(500, "An error occurred while retrieving user information");
        }
    }
}