
using Bobs_Corn_API.Services;
using System.Threading.RateLimiting;

namespace Bobs_Corn_API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddCors(opt =>
            {
                opt.AddDefaultPolicy(p =>
                    p.WithOrigins("http://localhost:4200")
                     .AllowAnyHeader()
                     .AllowAnyMethod());
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<TotalsStore>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        error = "Too Many Requests",
                        detail = "Limit: 1 corn per minute per client."
                    }, cancellationToken: token);
                };

                options.AddPolicy("per-client-1-per-minute", httpContext =>
                {
                    string clientKey = GetClientKey(httpContext);
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: clientKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromMinutes(1),
                            PermitLimit = 1,
                            QueueLimit = 0,
                            AutoReplenishment = true
                        });
                });
            });

            var app = builder.Build();

            app.UseCors();
            app.UseRateLimiter();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();

            static string GetClientKey(HttpContext ctx)
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
}
