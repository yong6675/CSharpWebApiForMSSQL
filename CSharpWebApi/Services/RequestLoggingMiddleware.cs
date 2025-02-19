using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

namespace CSharpWebApi.Services
{
    public class RequestLoggingMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

            using (LogContext.PushProperty("UserId", userId))
            {
                var startTime = Stopwatch.GetTimestamp();

                try
                {
                    var request = context.Request;
                    request.EnableBuffering(); // Allow multiple read requests

                    // read requests
                    var requestBody = "";
                    if (request.Method == HttpMethods.Post || request.Method == HttpMethods.Put)
                    {
                        using var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true);
                        requestBody = await reader.ReadToEndAsync();
                        request.Body.Position = 0;
                    }

                    Log.Information("Request {Method} {Url} started. Body: {RequestBody}",
                        request.Method, request.Path, requestBody);

                    await next(context);

                    var elapsedMs = GetElapsedMilliseconds(startTime, Stopwatch.GetTimestamp());
                    Log.Information("Request {Method} {Url} completed in {Elapsed} ms. Status: {StatusCode}",
                        request.Method, request.Path, elapsedMs, context.Response.StatusCode);
                }
                catch (Exception ex)
                {
                    var elapsedMs = GetElapsedMilliseconds(startTime, Stopwatch.GetTimestamp());
                    Log.Error(ex, "Request failed after {Elapsed} ms", elapsedMs);
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Internal server error");
                }
            }
        }

        private static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }
    }
}
