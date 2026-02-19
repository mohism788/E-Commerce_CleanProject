using E_Commerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("db")]
    public async Task<IActionResult> CheckDatabase()
    {
        try
        {
            bool canConnect = await _context.Database.CanConnectAsync();

            if (!canConnect)
                return StatusCode(500, "Database not reachable");

            // real query test
            await _context.Users.FirstOrDefaultAsync();

            return Ok(new
            {
                status = "Database is connected and responding",
                time = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                error = ex.Message
            });
        }
    }
}
