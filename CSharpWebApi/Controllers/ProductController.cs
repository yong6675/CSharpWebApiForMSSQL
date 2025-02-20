using CSharpWebApi.Data;
using CSharpWebApi.DTOs;
using CSharpWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace CSharpWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController(AppDbContext dbContext, ILogger<ProductController> logger) : Controller
    {
        // GET: api/GetProducts
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        [OutputCache(PolicyName = "Products")]
        [SwaggerOperation(Description = "Get products with pagination and sorting")]
        public async Task<ActionResult<PageResult<Product>>> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery, SwaggerParameter(Description = "Sort field (id/name/price)")] string sortBy = "id",
            [FromQuery] string order = "asc")
        {
            // Validate parameters
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 100) pageSize = 10;

            var query = dbContext.Products.AsNoTracking();

            // Sorting dynamically
            var allowedSortFields = new[] { "id", "name", "price" };
            var isValidSort = allowedSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);

            if (!isValidSort) sortBy = "id";

            query = order.ToLower() switch
            {
                "desc" => sortBy.ToLower() switch
                {
                    "price" => query.OrderByDescending(p => p.Price),
                    "name" => query.OrderByDescending(p => p.Name),
                    _ => query.OrderByDescending(p => p.Id)
                },
                _ => sortBy.ToLower() switch
                {
                    "price" => query.OrderBy(p => p.Price),
                    "name" => query.OrderBy(p => p.Name),
                    _ => query.OrderBy(p => p.Id)
                }
            };

            // Count total records
            var totalCount = await query.CountAsync();

            // Count pages
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Min(page, totalPages > 0 ? totalPages : 1);

            // Execute query
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            logger.LogInformation("Get Products is working.");

            return Ok(new PageResult<Product>
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageIndex = page,
                PageSize = pageSize,
                Data = items
            });
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
