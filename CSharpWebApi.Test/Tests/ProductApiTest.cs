using CSharpWebApi.Controllers;
using CSharpWebApi.Data;
using CSharpWebApi.DTOs;
using CSharpWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CSharpWebApi.Test.TestContext;
using System.ComponentModel.DataAnnotations;
using Serilog;
using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpWebApi.Test.Tests
{
    public class ProductApiTest : IDisposable
    {
        private readonly AppDbContext _dbDbContext;
        private readonly ProductController _controller;
        private readonly Mock<ILogger<ProductController>> _mockLogger;

        public ProductApiTest()
        {
            // Initialize Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/test-log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            _mockLogger = new Mock<ILogger<ProductController>>();

            _dbDbContext = InMemoryContextGenerator.Generate<AppDbContext>();
            _controller = new ProductController(_dbDbContext, _mockLogger.Object);

            // Initialize the in-memory database
            _dbDbContext.Products.AddRange(
                new Product { Id = 1, Name = "Product1", Price = 100 },
                new Product { Id = 2, Name = "Product2", Price = 200 }
            );
            _dbDbContext.SaveChanges();
        }

        [Fact]
        public async Task GetProducts_ListOfProducts()
        {
            var rand = new Random();
            for (var i = 3; i < 50; i++)
            {
                _dbDbContext.Products.Add(
                    new Product { Id = i, Name = $"Product{i}", Price = rand.Next(100,1001) }
                );
                await _dbDbContext.SaveChangesAsync();
            }

            // run request
            var result = await _controller.GetProducts();

            // validate result
            var actionResult = Assert.IsType<ActionResult<PageResult<Product>>>(result);
            Assert.NotNull(actionResult.Result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var products = Assert.IsType<PageResult<Product>>(okResult.Value);
            Assert.Equal(49, products.TotalCount);

            // validate log's output
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Get Products is working.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }

        [Fact]
        public async Task GetProduct_ReturnCorrectProduct()
        {
            // run request
            var result = await _controller.GetProduct(1);

            // validate result
            var actionResult = Assert.IsType<ActionResult<ProductDto>>(result);
            Assert.NotNull(actionResult.Result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var product = Assert.IsType<ProductDto>(okResult.Value);
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
            var newProduct = new ProductDto { Name = "Product3", Price = 300 };
            // run request
            var result = await _controller.CreateProduct(newProduct);

            // validate result
            var actionResult = Assert.IsType<ActionResult<ProductDto>>(result);
            Assert.NotNull(actionResult.Result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var product = Assert.IsType<Product>(createdAtActionResult.Value);
            Assert.Equal(newProduct.Name, product.Name);
            Assert.Equal(newProduct.Price, product.Price);

            // validate database
            var products = await _dbDbContext.Products.ToListAsync();
            Assert.Equal(3, products.Count);
        }

        [Fact]
        public void CreateProduct_AddWrongProduct()
        {
            var invalidProduct = new ProductDto { Name = "", Price = -10 };

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
            var product = await _dbDbContext.Products.FindAsync(1);
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
            var products = await _dbDbContext.Products.ToListAsync();
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

        public void Dispose()
        {
            // Clear Serilog
            Log.CloseAndFlush();
        }
    }
}
