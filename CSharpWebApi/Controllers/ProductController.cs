using CSharpWebApi.Data;
using CSharpWebApi.DTOs;
using CSharpWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSharpWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController(AppDbContext dbContext, ILogger<ProductController> logger) : Controller
    {
        // GET: api/GetProducts
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await dbContext.Products.ToListAsync();
            logger.LogInformation("Get Products is working.");
            return Ok(products);
        }

        // GET: api/GetProduct/5
        // <snippet_GetByID>
        [HttpGet("{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await dbContext.Products.FindAsync(id);

            if (product == null)
            {
                logger.LogError($"Failed to get product {id}.");
                return NotFound();
            }
            logger.LogInformation($"Product {id} get successfully.");
            return Ok(ProductToDTO(product));
        }

        // POST: api/CreateProduct
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] ProductDto dto)
        {
            var product = dto.DtoToProduct();
            if (product == null)
            {
                logger.LogError($"Failed to create product.");
                return NotFound();
            }

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();
            logger.LogInformation($"Product {product.Id} created successfully.");
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // PUT: api/UpdateProduct/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                logger.LogError($"Updated record Id not match with product data Id.");
                return BadRequest();
            }
            var existingProduct = await dbContext.Products.FindAsync(id);
            if (existingProduct == null)
            {
                logger.LogError($"Failed to update product {id}, not found record");
                return NotFound();
            }

            dbContext.Entry(existingProduct).CurrentValues.SetValues(product);

            try
            {
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Product {product.Id} updated successfully.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    logger.LogError($"Failed to update product {id}, not found record");
                    return NotFound();
                }
            }
            return NoContent();
        }

        // DELETE: api/DeleteProduct/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await dbContext.Products.FindAsync(id);
            if (product == null)
            {
                logger.LogError($"Failed to delete product {id}, not found record");
                return NotFound();
            }
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();
            logger.LogInformation($"Product {product.Id} delete successfully.");
            return NoContent();
        }


        private bool ProductExists(int id)
        {
            return dbContext.Products.AsNoTracking().Any(e => e.Id == id);
        }

        private static ProductDto ProductToDTO(Product todoItem) =>
            new()
            {
                Name = todoItem.Name,
                Price = todoItem.Price
            };

    }
}
