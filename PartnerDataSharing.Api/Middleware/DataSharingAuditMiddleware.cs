using System.Diagnostics;
using PartnerDataSharing.Api.Domain;
using PartnerDataSharing.Api.Services;

namespace PartnerDataSharing.Api.Middleware;

public sealed class DataSharingAuditMiddleware
{
    private readonly RequestDelegate _next;

    public DataSharingAuditMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, RequestContext requestContext, AuditEventStore auditStore)
    {
        var shouldAudit = ShouldAudit(context);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            if (shouldAudit && requestContext.Client is not null)
            {
                auditStore.Add(new AuditEvent(
                    Guid.NewGuid(),
                    DateTimeOffset.UtcNow,
                    requestContext.CorrelationId,
                    requestContext.Client.ClientName,
                    requestContext.Client.Role,
                    requestContext.Client.PartnerId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds));
            }
        }
    }

    private static bool ShouldAudit(HttpContext context)
    {
        var path = context.Request.Path;

        if (path.StartsWithSegments("/api/compliance/audit-events", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }
}
