using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
using server.Data;
using server.Models;
using server.Models.DTOs;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Clients
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Client>>> GetClients()
    {
        return await _context.Clients.ToListAsync();
    }

    // GET: api/Clients/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ClientDTO>> GetClient(int id)
    {
        var client = await _context.Clients
            .Include(c => c.Orders)
                .ThenInclude(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(c => c.ID == id);

        if (client == null)
        {
            return NotFound();
        }

        var clientDto = new ClientDTO
        {
            ID = client.ID,
            Nom = client.Nom,
            Prenom = client.Prenom,
            Email = client.Email,
            Adresse = client.Adresse,
            Telephone = client.Telephone,
            Orders = client.Orders.Select(o => new OrderDTO
            {
                ID = o.ID,
                ClientID = o.ClientID,
                DateCommande = o.DateCommande,
                Statut = o.Statut,
                Total = o.Total,
                OrderDetails = o.OrderDetails.Select(od => new OrderDetailDTO
                {
                    ID = od.ID,
                    CommandeID = od.CommandeID,
                    ProduitID = od.ProduitID,
                    Quantite = od.Quantite,
                    PrixUnitaire = od.PrixUnitaire,
                    Product = od.Product == null ? null : new ProductDTO
                    {
                        ID = od.Product.ID,
                        Nom = od.Product.Nom,
                        Description = od.Product.Description,
                        Prix = od.Product.Prix,
                        Stock = od.Product.Stock,
                        ImageURL = od.Product.ImageURL,
                        CategorieID = od.Product.CategorieID
                    }
                }).ToList()
            }).ToList()
        };

        return clientDto;
    }

    // POST: api/Clients
    [HttpPost]
    public async Task<ActionResult<Client>> CreateClient(Client client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetClient), new { id = client.ID }, client);
    }

    // PATCH: api/Clients/5
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchClient(int id, [FromBody] JsonPatchDocument<Client> patchDoc)
    {
        if (patchDoc == null)
        {
            return BadRequest();
        }

        var client = await _context.Clients.FindAsync(id);
        if (client == null)
        {
            return NotFound();
        }

        patchDoc.ApplyTo(client, ModelState);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ClientExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Clients/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null)
        {
            return NotFound();
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ClientExists(int id)
    {
        return _context.Clients.Any(e => e.ID == id);
    }
}