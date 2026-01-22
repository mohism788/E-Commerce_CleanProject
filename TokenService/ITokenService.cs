using E_Commerce.Models;

namespace E_Commerce.TokenService
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
