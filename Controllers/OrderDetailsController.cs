using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
using server.Data;
using server.Models;
using server.Models.DTOs;

namespace server.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Order Details")]
public class OrderDetailsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrderDetailsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all order details
    /// </summary>
    /// <returns>A list of order details with their associated products</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<OrderDetailDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDetailDTO>>> GetOrderDetails()
    {
        var orderDetails = await _context.OrderDetails
            .Include(od => od.Product)
            .ToListAsync();

        return orderDetails.Select(od => new OrderDetailDTO
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
        }).ToList();
    }

    /// <summary>
    /// Gets a specific order detail by ID
    /// </summary>
    /// <param name="id">The ID of the order detail to retrieve</param>
    /// <returns>The order detail with its associated product</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OrderDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailDTO>> GetOrderDetail(int id)
    {
        var orderDetail = await _context.OrderDetails
            .Include(od => od.Product)
            .FirstOrDefaultAsync(od => od.ID == id);

        if (orderDetail == null)
        {
            return NotFound();
        }

        return new OrderDetailDTO
        {
            ID = orderDetail.ID,
            CommandeID = orderDetail.CommandeID,
            ProduitID = orderDetail.ProduitID,
            Quantite = orderDetail.Quantite,
            PrixUnitaire = orderDetail.PrixUnitaire,
            Product = orderDetail.Product == null ? null : new ProductDTO
            {
                ID = orderDetail.Product.ID,
                Nom = orderDetail.Product.Nom,
                Description = orderDetail.Product.Description,
                Prix = orderDetail.Product.Prix,
                Stock = orderDetail.Product.Stock,
                ImageURL = orderDetail.Product.ImageURL,
                CategorieID = orderDetail.Product.CategorieID
            }
        };
    }

    /// <summary>
    /// Gets all order details for a specific order
    /// </summary>
    /// <param name="orderId">The ID of the order to get details for</param>
    /// <returns>A list of order details with their associated products</returns>
    [HttpGet("Order/{orderId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<OrderDetailDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDetailDTO>>> GetOrderDetailsByOrder(int orderId)
    {
        var orderDetails = await _context.OrderDetails
            .Include(od => od.Product)
            .Where(od => od.CommandeID == orderId)
            .ToListAsync();

        return orderDetails.Select(od => new OrderDetailDTO
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
        }).ToList();
    }

    /// <summary>
    /// Creates a new order detail
    /// </summary>
    /// <param name="orderDetail">The order detail to create</param>
    /// <returns>The newly created order detail</returns>
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OrderDetailDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDetailDTO>> CreateOrderDetail(OrderDetail orderDetail)
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

        // Clear the navigation property before adding
        orderDetail.Product = null;

        _context.OrderDetails.Add(orderDetail);
        await _context.SaveChangesAsync();

        // Load the product for the response
        await _context.Entry(orderDetail)
            .Reference(od => od.Product)
            .LoadAsync();

        // Convert to DTO for response
        var dto = new OrderDetailDTO
        {
            ID = orderDetail.ID,
            CommandeID = orderDetail.CommandeID,
            ProduitID = orderDetail.ProduitID,
            Quantite = orderDetail.Quantite,
            PrixUnitaire = orderDetail.PrixUnitaire,
            Product = orderDetail.Product == null ? null : new ProductDTO
            {
                ID = orderDetail.Product.ID,
                Nom = orderDetail.Product.Nom,
                Description = orderDetail.Product.Description,
                Prix = orderDetail.Product.Prix,
                Stock = orderDetail.Product.Stock,
                ImageURL = orderDetail.Product.ImageURL,
                CategorieID = orderDetail.Product.CategorieID
            }
        };

        return CreatedAtAction(nameof(GetOrderDetail), new { id = orderDetail.ID }, dto);
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