using System.ComponentModel.DataAnnotations;

namespace E_Commerce.DTOs.UserDTO
{
    public class UserRegisterDto
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        //role
       public string? Role { get; set; } = "Customer";




    }
}
