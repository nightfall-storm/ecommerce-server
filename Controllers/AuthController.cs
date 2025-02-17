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

        // Create new client
        var client = new Client
        {
            Nom = request.Nom,
            Prenom = request.Prenom,
            Email = request.Email,
            MotDePasse = _authService.HashPassword(request.MotDePasse),
            Adresse = request.Adresse ?? string.Empty,
            Telephone = request.Telephone ?? string.Empty
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
    /// Get current authenticated user's information
    /// </summary>
    /// <returns>User details</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(Client), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Client>> GetCurrentUser()
    {
        try
        {
            _logger.LogInformation("GetCurrentUser called. User Claims: {Claims}",
                string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));

            _logger.LogInformation("Authorization Header: {Auth}",
                HttpContext.Request.Headers["Authorization"].ToString());

            if (!User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("User is not authenticated");
                return Unauthorized("User is not authenticated");
            }

            // Try to find the user ID from multiple possible claim types
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        User.FindFirst("sub")?.Value;

            if (userId == null)
            {
                _logger.LogWarning("No user ID claim found in the token");
                return Unauthorized("No user ID claim found in the token");
            }

            _logger.LogInformation("Found user ID: {UserId}", userId);

            if (!int.TryParse(userId, out int userIdInt))
            {
                _logger.LogWarning("Failed to parse user ID: {UserId}", userId);
                return BadRequest("Invalid user ID format");
            }

            var client = await _context.Clients.FindAsync(userIdInt);
            if (client == null)
            {
                _logger.LogWarning("No client found with ID: {UserId}", userIdInt);
                return NotFound($"No user found with ID: {userIdInt}");
            }

            _logger.LogInformation("Successfully retrieved user: {UserId}", userIdInt);
            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCurrentUser: {Message}", ex.Message);
            return StatusCode(500, "An error occurred while retrieving user information");
        }
    }
}