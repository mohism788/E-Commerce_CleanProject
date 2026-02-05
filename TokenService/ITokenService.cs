using E_Commerce.Models;

namespace E_Commerce.TokenService
{
    public interface ITokenService
    {
        Task<string> CreateToken(User user);
    }
}
