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

    // PATCH: /Orders/5
    [HttpPatch("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PatchOrder(int id, [FromBody] OrderPatchDTO patchDTO)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        // Update only provided fields
        if (patchDTO.Statut != null) order.Statut = patchDTO.Statut;
        if (patchDTO.Total != null) order.Total = patchDTO.Total.Value;

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
            throw;
        }

        return NoContent();
    }

    // PATCH: /Orders/{id}/status
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateDTO statusUpdate)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(statusUpdate.Status))
        {
            return BadRequest("Status cannot be empty");
        }

        order.Statut = statusUpdate.Status;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Orders/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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