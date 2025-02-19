using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CSharpWebApi.Data;
using CSharpWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using CSharpWebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace CSharpWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController(AppDbContext dbContext, IConfiguration configuration) : Controller
    {
        [HttpPost]
        public IActionResult Register([FromBody] UserRegisterDto userRegister)
        {
            if (dbContext.Users.Any(u=>u.Username == userRegister.Username))
            {
                return BadRequest("Username already exists");
            }

            var user = new User
            {
                Username = userRegister.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(userRegister.Password),
                Role = userRegister.Role ?? "User"
            };

            dbContext.Users.Add(user); 
            dbContext.SaveChanges();
            
            return Ok("User registered successfully");
        }

        [HttpPost]
        public IActionResult Login([FromBody] UserLoginDto userLogin)
        {
            var existingUser = dbContext.Users.FirstOrDefault(u => u.Username == userLogin.Username);
            if (existingUser == null)
            {
                return BadRequest("Invalid username or password");
            }
            if (!BCrypt.Net.BCrypt.Verify(userLogin.Password, existingUser.Password))
            {
                return BadRequest("Invalid username or password");
            }
            var token = GenerateJwtToken(existingUser);
            return Ok(token);
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var user = dbContext.Users.Find(userId);

            if (user == null) return NotFound("User not found");

            return Ok(user?.UserToDto());
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? string.Empty);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                ]),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"] ?? string.Empty)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


    }
}
