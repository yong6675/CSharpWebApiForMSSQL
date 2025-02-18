using System.ComponentModel.DataAnnotations;
using CSharpWebApi.Models;

namespace CSharpWebApi.DTOs
{
    public class ProductDto
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0.")]
        public decimal Price { get; set; }

        public Product? DtoToProduct() =>
            new()
            {
                Name = Name,
                Price = Price
            };
    }
}
