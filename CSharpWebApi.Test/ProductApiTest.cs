using CSharpWebApi.Controllers;
using CSharpWebApi.Data;
using CSharpWebApi.DTOs;
using CSharpWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using CSharpWebApi.Test.TestContext;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace CSharpWebApi.Test
{
    public class ProductApiTest
    {
        private readonly ProductContext _dbContext;
        private readonly ProductController _controller;

        public ProductApiTest()
        {
            _dbContext = InMemoryContextGenerator.Generate<ProductContext>();
            _controller = new ProductController(_dbContext);

            // Initialize the in-memory database
            _dbContext.Products.AddRange(
                new Product { Id = 1, Name = "Product1", Price = 100 },
                new Product { Id = 2, Name = "Product2", Price = 200 }
            );
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task GetProducts_ListOfProducts()
        {
            // run request
            var result = await _controller.GetProducts();

            // validate result
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
            Assert.NotNull(actionResult.Result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var products = Assert.IsType<List<Product>>(okResult.Value);
            Assert.Equal(2, products.Count);
        }

        [Fact]
        public async Task GetProduct_ReturnCorrectProduct()
        {
            // run request
            var result = await _controller.GetProduct(1);

            // validate result
            var actionResult = Assert.IsType<ActionResult<ProductDTO>>(result);
            Assert.NotNull(actionResult.Result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var product = Assert.IsType<ProductDTO>(okResult.Value);
            Assert.Equal("Product1", product.Name);
        }

        [Fact]
        public async Task GetProduct_WhenNotFound()
        {
            // run request
            var result = await _controller.GetProduct(3);

            // validate result
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateProduct_AddCorrectProduct()
        {
            var newProduct = new ProductDTO { Name = "Product3", Price = 300 };
            // run request
            var result = await _controller.CreateProduct(newProduct);

            // validate result
            var actionResult = Assert.IsType<ActionResult<ProductDTO>>(result);
            Assert.NotNull(actionResult.Result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var product = Assert.IsType<Product>(createdAtActionResult.Value);
            Assert.Equal(newProduct.Name, product.Name);
            Assert.Equal(newProduct.Price, product.Price);

            // validate database
            var products = await _dbContext.Products.ToListAsync();
            Assert.Equal(3, products.Count);
        }

        [Fact]
        public void CreateProduct_AddWrongProduct()
        {
            var invalidProduct = new ProductDTO { Name = "", Price = -10 };

            // **手动触发 ModelState 验证**
            _controller.ModelState.Clear();
            var validationContext = new ValidationContext(invalidProduct, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(invalidProduct, validationContext, validationResults, true);

            foreach (var validationResult in validationResults)
            {
                if (validationResult.ErrorMessage != null)
                    _controller.ModelState.AddModelError(validationResult.MemberNames.First(),
                        validationResult.ErrorMessage);
            }

            if (!_controller.ModelState.IsValid) return;
            // Act
            var result = _controller.CreateProduct(invalidProduct);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateProduct_UpdateCorrectProduct()
        {
            var updatedProduct = new Product { Id = 1, Name = "Product1Updated", Price = 150 };
            // run request
            var result = await _controller.UpdateProduct(updatedProduct.Id, updatedProduct);
            // validate result
            Assert.IsType<NoContentResult>(result);
            // validate database
            var product = await _dbContext.Products.FindAsync(1);
            Assert.Equal(updatedProduct.Name, product?.Name);
            Assert.Equal(updatedProduct.Price, product?.Price);
        }

        [Fact]
        public async Task UpdateProduct_WhenIdNotMatch()
        {
            var updatedProduct = new Product { Id = 1, Name = "Product1Updated", Price = 150 };
            // run request
            var result = await _controller.UpdateProduct(2, updatedProduct);
            // validate result
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task UpdateProduct_WhenNotFound()
        {
            var updatedProduct = new Product { Id = 3, Name = "Product3", Price = 300 };
            // run request
            var result = await _controller.UpdateProduct(updatedProduct.Id, updatedProduct);
            // validate result
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_DeleteCorrectProduct()
        {
            // run request
            var result = await _controller.DeleteProduct(1);
            // validate result
            Assert.IsType<NoContentResult>(result);
            // validate database
            var products = await _dbContext.Products.ToListAsync();
            Assert.Single(products);
        }

        [Fact]
        public async Task DeleteProduct_WhenNotFound()
        {
            // run request
            var result = await _controller.DeleteProduct(3);
            // validate result
            Assert.IsType<NotFoundResult>(result);
        }

    }
}
