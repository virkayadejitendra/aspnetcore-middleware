using PartnerDataSharing.Api.Services;

namespace PartnerDataSharing.Api.Middleware;

public sealed class ApiKeyAuthenticationMiddleware
{
    private const string ApiKeyHeader = "X-Api-Key";
    private const string CorrelationIdHeader = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, DemoDataStore dataStore, RequestContext requestContext)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        requestContext.CorrelationId = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        if (IsPublicEndpoint(context))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKeyValues) ||
            string.IsNullOrWhiteSpace(apiKeyValues.FirstOrDefault()))
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Missing API key.");
            return;
        }

        var client = dataStore.FindClient(apiKeyValues.First()!);
        if (client is null)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Invalid API key.");
            return;
        }

        requestContext.Client = client;
        context.Items["ClientName"] = client.ClientName;
        context.Items["Role"] = client.Role.ToString();
        context.Items["PartnerId"] = client.PartnerId;

        await _next(context);
    }

    private static bool IsPublicEndpoint(HttpContext context) =>
        context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var values) &&
            !string.IsNullOrWhiteSpace(values.FirstOrDefault()))
        {
            return values.First()!;
        }

        return Guid.NewGuid().ToString("N");
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new { error = message });
    }
}
