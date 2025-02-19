using CSharpWebApi.Controllers;
using CSharpWebApi.Data;
using CSharpWebApi.Test.TestContext;
using CSharpWebApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CSharpWebApi.Test.Tests
{
    public class UserApiTest
    {
        private readonly AppDbContext _dbDbContext;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly UserController _controller;

        public UserApiTest()
        {
            _dbDbContext = InMemoryContextGenerator.Generate<AppDbContext>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Simulate JWT configuration of IConfiguration
            var jwtSettings = new Dictionary<string, string>
            {
                { "Jwt:Key", "7iWlKC1dIqvdbW48Jt1mJTxQmnsvYa6d" },
                { "Jwt:Issuer", "https://localhost:7069" },
                { "Jwt:Audience", "https://localhost:7069" },
                { "Jwt:ExpireMinutes", "60" }
            };

            // Simulate methods of IConfiguration.GetSection
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            mockConfigurationSection.Setup(x => x["Key"]).Returns(jwtSettings["Jwt:Key"]);
            mockConfigurationSection.Setup(x => x["Issuer"]).Returns(jwtSettings["Jwt:Issuer"]);
            mockConfigurationSection.Setup(x => x["Audience"]).Returns(jwtSettings["Jwt:Audience"]);
            mockConfigurationSection.Setup(x => x["ExpireMinutes"]).Returns(jwtSettings["Jwt:ExpireMinutes"]);

            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x.GetSection("Jwt")).Returns(mockConfigurationSection.Object);

            _controller = new UserController(_dbDbContext, _mockConfiguration.Object);
        }

        [Fact]
        public void RegisterUser_UserRegistered()
        {
            var userRegister = new UserRegisterDto
            {
                Username = "TestUser",
                Password = "TestPassword"
            };
            // run request
            var result = _controller.Register(userRegister);
            // validate result
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void RegisterUser_UsernameExists()
        {
            var userRegister = new UserRegisterDto
            {
                Username = "TestUser",
                Password = "TestPassword"
            };
            // run request
            _controller.Register(userRegister);
            var result = _controller.Register(userRegister);
            // validate result
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void LoginUser_UserLoggedIn()
        {
            var userRegister = new UserRegisterDto
            {
                Username = "TestUser",
                Password = "TestPassword"
            };
            // run request
            _controller.Register(userRegister);
            var userLogin = new UserLoginDto
            {
                Username = "TestUser",
                Password = "TestPassword"
            };
            var result = _controller.Login(userLogin);
            // validate result
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void LoginUser_InvalidUsername()
        {
            var userRegister = new UserRegisterDto
            {
                Username = "TestUser",
                Password = "TestPassword"
            };
            // run request
            _controller.Register(userRegister);
            var userLogin = new UserLoginDto
            {
                Username = "InvalidUsername",
                Password = "TestPassword"
            };
            var result = _controller.Login(userLogin);
            // validate result
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void LoginUser_InvalidPassword()
        {
            var userRegister = new UserRegisterDto
            {
                Username = "TestUser",
                Password = "TestPassword"
            };
            // run request
            _controller.Register(userRegister);
            var userLogin = new UserLoginDto
            {
                Username = "TestUser",
                Password = "InvalidPassword"
            };
            var result = _controller.Login(userLogin);
            // validate result
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void GetCurrentUser_IsAuthorize()
        {
            // Simulate user authorized
            var user = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User")
            ], "TestAuthentication"));

            var controller = new UserController(_dbDbContext, _mockConfiguration.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };

            var userRegister = new UserRegisterDto
            {
                Username = "TestUser",
                Password = "TestPassword"
            };
            // run request
            controller.Register(userRegister);
            var result = controller.GetCurrentUser();

            // validate result
            var okResult = result as OkObjectResult;
            dynamic userInfo = okResult.Value;
            Assert.Equal("TestUser", userInfo.Username);
            Assert.Equal("User", userInfo.Role);
        }

        [Fact]
        public void GetCurrentUser_UserNotFound()
        {
            // Simulate user authorized
            var user = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, "2"),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "User")
            ], "TestAuthentication"));

            var controller = new UserController(_dbDbContext, _mockConfiguration.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };

            var userRegister = new UserRegisterDto
            {
                Username = "Test",
                Password = "TestPassword"
            };
            // run request
            controller.Register(userRegister);
            var result = controller.GetCurrentUser();

            // validate result
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
