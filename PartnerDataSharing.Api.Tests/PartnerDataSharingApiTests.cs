using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PartnerDataSharing.Api.Tests;

public sealed class PartnerDataSharingApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PartnerDataSharingApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpoint_WorksWithoutApiKey()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutApiKey_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidApiKey_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "bad-key");

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RetailPartner_CanAccessOwnOrders()
    {
        var client = CreateClient("retail-demo-key");

        var response = await client.GetAsync("/api/partners/PARTNER-RETAIL-001/orders");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RetailPartner_CannotAccessAnotherPartnersOrders()
    {
        var client = CreateClient("retail-demo-key");

        var response = await client.GetAsync("/api/partners/PARTNER-DIST-001/orders");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DistributorPartner_CanCreateAndViewOwnOrders()
    {
        var client = CreateClient("distributor-demo-key");

        var createResponse = await client.PostAsJsonAsync(
            "/api/partners/PARTNER-DIST-001/orders",
            new { productId = "PROD-001", quantity = 5 });
        var getResponse = await client.GetAsync("/api/partners/PARTNER-DIST-001/orders");

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task AnalyticsPartner_CanReadAggregatedSalesOnly()
    {
        var client = CreateClient("analytics-demo-key");

        var analyticsResponse = await client.GetAsync("/api/analytics/sales-summary");
        var ordersResponse = await client.GetAsync("/api/partners/PARTNER-RETAIL-001/orders");

        Assert.Equal(HttpStatusCode.OK, analyticsResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, ordersResponse.StatusCode);
    }

    [Fact]
    public async Task ComplianceUser_CanReadAuditEvents()
    {
        var analyticsClient = CreateClient("analytics-demo-key");
        analyticsClient.DefaultRequestHeaders.Add("X-Correlation-Id", "test-correlation-001");
        await analyticsClient.GetAsync("/api/analytics/sales-summary");

        var complianceClient = CreateClient("compliance-demo-key");
        var response = await complianceClient.GetAsync("/api/compliance/audit-events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("test-correlation-001", body);
        Assert.Contains("/api/analytics/sales-summary", body);
    }

    [Fact]
    public async Task ComplianceUser_ReadingAuditEvents_ReturnsApiEventsButDoesNotAuditTheReadRequest()
    {
        var productsCorrelationId = $"products-{Guid.NewGuid():N}";
        var inventoryCorrelationId = $"inventory-{Guid.NewGuid():N}";
        var ordersCorrelationId = $"orders-{Guid.NewGuid():N}";
        var analyticsCorrelationId = $"analytics-{Guid.NewGuid():N}";
        var auditReadCorrelationId = $"audit-read-{Guid.NewGuid():N}";

        var retailClient = CreateClient("retail-demo-key");
        retailClient.DefaultRequestHeaders.Add("X-Correlation-Id", productsCorrelationId);
        await retailClient.GetAsync("/api/products");

        retailClient.DefaultRequestHeaders.Remove("X-Correlation-Id");
        retailClient.DefaultRequestHeaders.Add("X-Correlation-Id", inventoryCorrelationId);
        await retailClient.GetAsync("/api/inventory");

        retailClient.DefaultRequestHeaders.Remove("X-Correlation-Id");
        retailClient.DefaultRequestHeaders.Add("X-Correlation-Id", ordersCorrelationId);
        await retailClient.GetAsync("/api/partners/PARTNER-RETAIL-001/orders");

        var analyticsClient = CreateClient("analytics-demo-key");
        analyticsClient.DefaultRequestHeaders.Add("X-Correlation-Id", analyticsCorrelationId);
        await analyticsClient.GetAsync("/api/analytics/sales-summary");

        var complianceClient = CreateClient("compliance-demo-key");
        complianceClient.DefaultRequestHeaders.Add("X-Correlation-Id", auditReadCorrelationId);

        var response = await complianceClient.GetAsync("/api/compliance/audit-events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains(productsCorrelationId, body);
        Assert.Contains("/api/products", body);
        Assert.Contains(inventoryCorrelationId, body);
        Assert.Contains("/api/inventory", body);
        Assert.Contains(ordersCorrelationId, body);
        Assert.Contains("/api/partners/PARTNER-RETAIL-001/orders", body);
        Assert.Contains(analyticsCorrelationId, body);
        Assert.Contains("/api/analytics/sales-summary", body);
        Assert.DoesNotContain(auditReadCorrelationId, body);
    }

    private HttpClient CreateClient(string apiKey)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        return client;
    }
}
