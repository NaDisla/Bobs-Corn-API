namespace Bobs_Corn_API.Services
{
    public class ClientKeyResolver
    {
        public string Resolve(HttpContext ctx)
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
