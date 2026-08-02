using PartnerDataSharing.Api.Domain;
using PartnerDataSharing.Api.Services;

namespace PartnerDataSharing.Api.Middleware;

public sealed class PartnerAccessMiddleware
{
    private readonly RequestDelegate _next;

    public PartnerAccessMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, RequestContext requestContext)
    {
        if (IsPublicEndpoint(context))
        {
            await _next(context);
            return;
        }

        if (requestContext.Client is null)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, "Authenticated client context is missing.");
            return;
        }

        var path = context.Request.Path;
        var role = requestContext.Client.Role;

        if (path.StartsWithSegments("/api/products", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWithSegments("/api/inventory", StringComparison.OrdinalIgnoreCase))
        {
            if (role is PartnerRole.RetailPartner or PartnerRole.DistributorPartner or PartnerRole.InternalAdmin)
            {
                await _next(context);
                return;
            }

            await WriteProblemAsync(context, StatusCodes.Status403Forbidden, "Role cannot access product or inventory APIs.");
            return;
        }

        if (path.StartsWithSegments("/api/partners", StringComparison.OrdinalIgnoreCase))
        {
            await ValidatePartnerEndpointAsync(context, requestContext);
            return;
        }

        if (path.StartsWithSegments("/api/analytics", StringComparison.OrdinalIgnoreCase))
        {
            if (role is PartnerRole.AnalyticsPartner or PartnerRole.InternalAdmin)
            {
                await _next(context);
                return;
            }

            await WriteProblemAsync(context, StatusCodes.Status403Forbidden, "Role cannot access analytics APIs.");
            return;
        }

        if (path.StartsWithSegments("/api/compliance", StringComparison.OrdinalIgnoreCase))
        {
            if (role is PartnerRole.ComplianceUser or PartnerRole.InternalAdmin)
            {
                await _next(context);
                return;
            }

            await WriteProblemAsync(context, StatusCodes.Status403Forbidden, "Role cannot access compliance APIs.");
            return;
        }

        await _next(context);
    }

    private async Task ValidatePartnerEndpointAsync(HttpContext context, RequestContext requestContext)
    {
        var role = requestContext.Client!.Role;
        if (role is not (PartnerRole.RetailPartner or PartnerRole.DistributorPartner or PartnerRole.InternalAdmin))
        {
            await WriteProblemAsync(context, StatusCodes.Status403Forbidden, "Role cannot access partner order APIs.");
            return;
        }

        var routePartnerId = context.Request.RouteValues["partnerId"]?.ToString();
        var headerPartnerId = context.Request.Headers["X-Partner-Id"].FirstOrDefault();
        var requestedPartnerId = routePartnerId ?? headerPartnerId;

        if (string.IsNullOrWhiteSpace(requestedPartnerId))
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Partner context is required.");
            return;
        }

        if (role == PartnerRole.InternalAdmin || requestContext.PartnerId == requestedPartnerId)
        {
            await _next(context);
            return;
        }

        await WriteProblemAsync(context, StatusCodes.Status403Forbidden, "Partner can access only its own data.");
    }

    private static bool IsPublicEndpoint(HttpContext context) =>
        context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new { error = message });
    }
}
