namespace E_Commerce.DTOs.UserDTO
{
    public class NewUserDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }

        //role
        public string Role { get; set; }
        public Guid Id { get; set; }

    }
}
