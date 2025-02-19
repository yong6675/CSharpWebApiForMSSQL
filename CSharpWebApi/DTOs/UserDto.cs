using System.ComponentModel.DataAnnotations;

namespace CSharpWebApi.DTOs
{
    public class UserDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public class UserRegisterDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public class UserLoginDto
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
