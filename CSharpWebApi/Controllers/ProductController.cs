using CSharpWebApi.Data;
using CSharpWebApi.DTOs;
using CSharpWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSharpWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController(ProductContext context) : Controller
    {
        // GET: api/GetProducts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await context.Products.ToListAsync();
            return Ok(products);
        }

        // GET: api/GetProduct/5
        // <snippet_GetByID>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await context.Products.FindAsync(id);

            if (product == null) return NotFound();

            return Ok(ProductToDTO(product));
        }

        // GET: api/CreateProduct
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> CreateProduct([FromBody] ProductDTO dto)
        {
            var product = dto.DTOToProduct();
            if (product == null) return NotFound();

            context.Products.Add(product);
            await context.SaveChangesAsync();

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
            var existingProduct = await context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            context.Entry(existingProduct).CurrentValues.SetValues(product);

            //_context.Entry(product).State = EntityState.Modified;
            try
            {
                await context.SaveChangesAsync();
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
            var product = await context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            return NoContent();
        }


        private bool ProductExists(int id)
        {
            return context.Products.AsNoTracking().Any(e => e.Id == id);
        }

        private static ProductDTO ProductToDTO(Product todoItem) =>
            new()
            {
                Name = todoItem.Name,
                Price = todoItem.Price
            };

    }
}
