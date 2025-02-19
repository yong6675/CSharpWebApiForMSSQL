using System.ComponentModel.DataAnnotations;
using CSharpWebApi.DTOs;

namespace CSharpWebApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; }
        public string Role { get; set; } = "User";

        public UserDto? UserToDto() =>
            new()
            {
                Username = Username,
                Role = Role
            };
    }
}
