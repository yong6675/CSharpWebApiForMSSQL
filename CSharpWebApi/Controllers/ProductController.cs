using CSharpWebApi.Data;
using CSharpWebApi.DTOs;
using CSharpWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSharpWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController(AppDbContext dbContext) : Controller
    {
        // GET: api/GetProducts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await dbContext.Products.ToListAsync();
            return Ok(products);
        }

        // GET: api/GetProduct/5
        // <snippet_GetByID>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await dbContext.Products.FindAsync(id);

            if (product == null) return NotFound();

            return Ok(ProductToDTO(product));
        }

        // POST: api/CreateProduct
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] ProductDto dto)
        {
            var product = dto.DtoToProduct();
            if (product == null) return NotFound();

            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // PUT: api/UpdateProduct/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }
            var existingProduct = await dbContext.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            dbContext.Entry(existingProduct).CurrentValues.SetValues(product);

            //_context.Entry(product).State = EntityState.Modified;
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
            }
            return NoContent();
        }

        // DELETE: api/DeleteProduct/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await dbContext.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();
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
