using Bobs_Corn_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Bobs_Corn_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CornController : ControllerBase
    {
        private readonly TotalsStore _totals;
        private readonly ClientKeyResolver _resolver;
        public CornController(TotalsStore totals, ClientKeyResolver resolver)
        {
            _totals = totals;
            _resolver = resolver;
        }

        [HttpPost("buy")]
        [EnableRateLimiting("per-client-1-per-minute")]
        public IActionResult Buy()
        {
            var clientId = _resolver.Resolve(HttpContext);
            var total = _totals.Increment(clientId);
            return Ok(new { message = "🌽", clientId, totalBought = total, timestamp = DateTime.UtcNow });
        }
    }
}
