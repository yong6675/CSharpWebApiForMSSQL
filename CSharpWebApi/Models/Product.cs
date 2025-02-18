using System.ComponentModel.DataAnnotations;
using CSharpWebApi.DTOs;

namespace CSharpWebApi.Models;

public class Product
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than or equal to 0.")]
    public decimal Price { get; set; }

    public ProductDTO? ProductToDTO() =>
        new()
        {
            Name = Name,
            Price = Price
        };
}