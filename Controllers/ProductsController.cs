using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;
using server.Data;
using server.Models;
using server.Models.DTOs;
using server.Services;

namespace server.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly FileUploadService _fileUploadService;

    public ProductsController(
        ApplicationDbContext context,
        FileUploadService fileUploadService)
    {
        _context = context;
        _fileUploadService = fileUploadService;
    }

    // GET: /Products
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return await _context.Products.ToListAsync();
    }

    // GET: api/Products/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        return product;
    }

    // POST: /Products
    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Product>> CreateProduct([FromForm] ProductCreateDTO productDTO)
    {
        try
        {
            var product = new Product
            {
                Nom = productDTO.Nom,
                Description = productDTO.Description,
                Prix = productDTO.Prix,
                Stock = productDTO.Stock,
                CategorieID = productDTO.CategorieID
            };

            if (productDTO.Image != null)
            {
                product.ImageURL = await _fileUploadService.SaveProductImage(productDTO.Image);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.ID }, product);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PATCH: /Products/5
    [HttpPatch("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchProduct(int id, [FromForm] ProductPatchDTO patchDTO)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // Update fields if provided
        if (patchDTO.Nom != null) product.Nom = patchDTO.Nom;
        if (patchDTO.Description != null) product.Description = patchDTO.Description;
        if (patchDTO.Prix != null) product.Prix = patchDTO.Prix.Value;
        if (patchDTO.Stock != null) product.Stock = patchDTO.Stock.Value;
        if (patchDTO.CategorieID != null) product.CategorieID = patchDTO.CategorieID.Value;

        // Handle image update
        if (patchDTO.Image != null)
        {
            // Delete old image if exists
            _fileUploadService.DeleteProductImage(product.ImageURL);
            // Save new image
            product.ImageURL = await _fileUploadService.SaveProductImage(patchDTO.Image);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: /Products/5
    [HttpDelete("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // Delete product image if exists
        _fileUploadService.DeleteProductImage(product.ImageURL);

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.ID == id);
    }
}