using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class CustomCorsMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var origin = context.Request.Headers["Origin"].ToString();
        
        if (IsValidOrigin(origin))
        {
            context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
            context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
            context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        }

        if (context.Request.Method == "OPTIONS")
        {
            context.Response.StatusCode = 204; // No content for preflight requests
            await context.Response.CompleteAsync();
            return;
        }

        await _next(context);
    }

    private static bool IsValidOrigin(string origin)
    {   
        Console.WriteLine(origin);
        return true;
    }
}
