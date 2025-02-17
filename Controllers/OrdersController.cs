using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using server.Data;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
            .ToListAsync();
    }

    // GET: api/Orders/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.ID == id);

        if (order == null)
        {
            return NotFound();
        }

        return order;
    }

    // GET: api/Orders/Client/5
    [HttpGet("Client/{clientId}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByClient(int clientId)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
            .Where(o => o.ClientID == clientId)
            .ToListAsync();
    }

    // POST: api/Orders
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(Order order)
    {
        // Set the order date to current time if not specified
        if (order.DateCommande == default)
        {
            order.DateCommande = DateTime.UtcNow;
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = order.ID }, order);
    }

    // PATCH: api/Orders/5
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchOrder(int id, [FromBody] JsonPatchDocument<Order> patchDoc)
    {
        if (patchDoc == null)
        {
            return BadRequest();
        }

        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        patchDoc.ApplyTo(order, ModelState);
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
            if (!OrderExists(id))
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

    // PATCH: api/Orders/5/status
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> PatchOrderStatus(int id, [FromBody] string status)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        order.Statut = status;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Orders/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.ID == id);

        if (order == null)
        {
            return NotFound();
        }

        _context.OrderDetails.RemoveRange(order.OrderDetails);
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool OrderExists(int id)
    {
        return _context.Orders.Any(e => e.ID == id);
    }
}