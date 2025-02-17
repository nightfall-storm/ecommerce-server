using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using server.Data;
using server.Models;

namespace server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderDetailsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrderDetailsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/OrderDetails
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDetail>>> GetOrderDetails()
    {
        return await _context.OrderDetails
            .Include(od => od.Product)
            .ToListAsync();
    }

    // GET: api/OrderDetails/5
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDetail>> GetOrderDetail(int id)
    {
        var orderDetail = await _context.OrderDetails
            .Include(od => od.Product)
            .FirstOrDefaultAsync(od => od.ID == id);

        if (orderDetail == null)
        {
            return NotFound();
        }

        return orderDetail;
    }

    // GET: api/OrderDetails/Order/5
    [HttpGet("Order/{orderId}")]
    public async Task<ActionResult<IEnumerable<OrderDetail>>> GetOrderDetailsByOrder(int orderId)
    {
        return await _context.OrderDetails
            .Include(od => od.Product)
            .Where(od => od.CommandeID == orderId)
            .ToListAsync();
    }

    // POST: api/OrderDetails
    [HttpPost]
    public async Task<ActionResult<OrderDetail>> CreateOrderDetail(OrderDetail orderDetail)
    {
        // Get the product to validate and set the unit price
        var product = await _context.Products.FindAsync(orderDetail.ProduitID);
        if (product == null)
        {
            return BadRequest("Invalid product ID");
        }

        // Set the unit price from the product
        orderDetail.PrixUnitaire = product.Prix;

        // Validate stock
        if (product.Stock < orderDetail.Quantite)
        {
            return BadRequest("Insufficient stock");
        }

        // Update product stock
        product.Stock -= orderDetail.Quantite;

        _context.OrderDetails.Add(orderDetail);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrderDetail), new { id = orderDetail.ID }, orderDetail);
    }

    // PATCH: api/OrderDetails/5
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchOrderDetail(int id, [FromBody] JsonPatchDocument<OrderDetail> patchDoc)
    {
        if (patchDoc == null)
        {
            return BadRequest();
        }

        var orderDetail = await _context.OrderDetails.FindAsync(id);
        if (orderDetail == null)
        {
            return NotFound();
        }

        // Store original quantity for stock calculation
        int originalQuantity = orderDetail.Quantite;

        patchDoc.ApplyTo(orderDetail, ModelState);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // If quantity was changed, update product stock
        if (originalQuantity != orderDetail.Quantite)
        {
            var product = await _context.Products.FindAsync(orderDetail.ProduitID);
            if (product == null)
            {
                return BadRequest("Invalid product ID");
            }

            int quantityDifference = orderDetail.Quantite - originalQuantity;
            if (product.Stock < quantityDifference)
            {
                return BadRequest("Insufficient stock");
            }

            product.Stock -= quantityDifference;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!OrderDetailExists(id))
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

    // DELETE: api/OrderDetails/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrderDetail(int id)
    {
        var orderDetail = await _context.OrderDetails.FindAsync(id);
        if (orderDetail == null)
        {
            return NotFound();
        }

        // Restore product stock
        var product = await _context.Products.FindAsync(orderDetail.ProduitID);
        if (product != null)
        {
            product.Stock += orderDetail.Quantite;
        }

        _context.OrderDetails.Remove(orderDetail);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool OrderDetailExists(int id)
    {
        return _context.OrderDetails.Any(e => e.ID == id);
    }
}