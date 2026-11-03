using Microsoft.AspNetCore.Mvc;

namespace BlockedUserMiddleware;

public sealed class UserBlockingMiddleware
{
    private const string UserIdHeaderName = "X-User-Id";

    private static readonly HashSet<Guid> BlockedUserIds =
    [
        Guid.Parse("00000000-0000-0000-0000-000000000001")
    ];

    private readonly RequestDelegate _next;

    public UserBlockingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(UserIdHeaderName, out var headerValue) &&
            Guid.TryParse(headerValue.ToString(), out var userId) &&
            BlockedUserIds.Contains(userId))
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "User Blocked",
                Detail = "Ваш акаунт заблоковано за порушення правил сервісу.",
                Instance = context.Request.Path
            };

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problemDetails);
            return;
        }

        await _next(context);
    }
}
