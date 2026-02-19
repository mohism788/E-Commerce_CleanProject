using E_Commerce.DTOs.UserDTO;
using E_Commerce.Models;
using E_Commerce.TokenService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;

        private readonly SignInManager<User> _signInManager;

        public UserController(UserManager<User> userManager, ITokenService tokenService, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
        }

        //regist
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
        {
            try
            {

                var usernameExists = await _userManager.FindByNameAsync(userRegisterDto.Username);
                if (usernameExists != null)
                {
                    return new BadRequestObjectResult("User already exists!");
                }
                User user = new User()
                {
                    UserName = userRegisterDto.Username,
                    Email = userRegisterDto.Email,
                    CreatedAt = DateTime.UtcNow,


                };
                var result = await _userManager.CreateAsync(user, userRegisterDto.Password);
                if (!result.Succeeded)
                {
                    return new BadRequestObjectResult("User creation failed! Please check user details and try again.");
                }

                // Validate and assign role
                var role = userRegisterDto.Role?.Trim() ?? "Customer";

                // Check if role exists
                var roleExists = await _userManager.GetRolesAsync(user);
          
                // Assign role to user
                var roleResult = await _userManager.AddToRoleAsync(user, role);

                if (!roleResult.Succeeded)
                {
                    // If role doesn't exist, assign default "Customer" role
                    await _userManager.AddToRoleAsync(user, "Customer");
                    role = "Customer"; // Update role variable
                }

                // Get user roles for token
                var userRoles = await _userManager.GetRolesAsync(user);

                return new OkObjectResult(
                    new NewUserDto
                    {
                        UserName = user.UserName,
                        Email = user.Email,
                        Token = await _tokenService.CreateToken(user),
                        Role = role,
                        Id = Guid.Parse(user.Id.ToString())
                    });
            }
            catch (Exception ex)
            {
                return new ObjectResult("Internal server error: " + ex.Message) { StatusCode = 500 };
            }
        }

        //login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userLoginDto.UserName);
                if (user == null)
                {
                    return new BadRequestObjectResult("Invalid username or password!");
                }
                var result = await _signInManager.CheckPasswordSignInAsync(user, userLoginDto.Password, false);
                if (!result.Succeeded)
                {
                    return new BadRequestObjectResult("Invalid username or password!");
                }


                            return new OkObjectResult(
                    new NewUserDto
                    {
                        UserName = user.UserName,
                        Token = await _tokenService.CreateToken(user),
                        Email = user.Email,
                        Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Customer",
                        Id = user.Id
                    });
            }
            catch (Exception ex)
            {
                return new ObjectResult("Internal server error: " + ex.Message) { StatusCode = 500 };
            }
        }
    }
}
