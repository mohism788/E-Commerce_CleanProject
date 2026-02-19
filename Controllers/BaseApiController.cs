using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Extracts the current user's ID from the JWT token claims.
        /// </summary>
        /// <returns>The user's Guid ID</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authenticated or the token is invalid</exception>
        protected Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            return userId;
        }
    }
}
