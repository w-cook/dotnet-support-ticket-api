using DotnetSupportTicketApi.Data;
using Microsoft.AspNetCore.Mvc;

namespace DotnetSupportTicketApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public HealthController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "API is running",
                timestampUtc = DateTime.UtcNow,
                databaseProvider = _dbContext.Database.ProviderName
            });
        }
    }
}