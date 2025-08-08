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

        public CornController(TotalsStore totals)
        {
            _totals = totals;
        }

        [HttpPost("buy")]
        [EnableRateLimiting("per-client-1-per-minute")]
        public IActionResult Buy()
        {
            var clientId = GetClientId(HttpContext);
            var total = _totals.Increment(clientId);

            return Ok(new
            {
                message = "🌽",
                clientId,
                totalBought = total,
                timestamp = DateTime.UtcNow
            });
        }

        private static string GetClientId(HttpContext ctx)
        {
            if (ctx.Request.Headers.TryGetValue("X-Client-Id", out var values) &&
                !string.IsNullOrWhiteSpace(values.ToString()))
            {
                return values.ToString();
            }

            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            return string.IsNullOrWhiteSpace(ip) ? "anonymous" : ip!;
        }
    }
}
