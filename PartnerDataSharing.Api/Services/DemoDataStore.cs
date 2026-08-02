using PartnerDataSharing.Api.Domain;

namespace PartnerDataSharing.Api.Services;

public sealed class DemoDataStore
{
    private readonly List<PartnerOrder> _orders =
    [
        new("ORD-1001", "PARTNER-RETAIL-001", "PROD-001", 30, new DateOnly(2026, 7, 20), "Submitted"),
        new("ORD-1002", "PARTNER-RETAIL-001", "PROD-002", 12, new DateOnly(2026, 7, 21), "Confirmed"),
        new("ORD-2001", "PARTNER-DIST-001", "PROD-003", 80, new DateOnly(2026, 7, 22), "Dispatched")
    ];

    private readonly object _ordersLock = new();

    public IReadOnlyList<ApiClient> ApiClients { get; } =
    [
        new("retail-demo-key", "Retail Demo Client", PartnerRole.RetailPartner, "PARTNER-RETAIL-001"),
        new("distributor-demo-key", "Distributor Demo Client", PartnerRole.DistributorPartner, "PARTNER-DIST-001"),
        new("analytics-demo-key", "Analytics Demo Client", PartnerRole.AnalyticsPartner, null),
        new("compliance-demo-key", "Compliance Demo Client", PartnerRole.ComplianceUser, null),
        new("admin-demo-key", "Internal Admin Demo Client", PartnerRole.InternalAdmin, null)
    ];

    public IReadOnlyList<Partner> Partners { get; } =
    [
        new("PARTNER-RETAIL-001", "Retail Partner One", PartnerRole.RetailPartner, "North"),
        new("PARTNER-RETAIL-002", "Retail Partner Two", PartnerRole.RetailPartner, "West"),
        new("PARTNER-DIST-001", "Distributor Partner One", PartnerRole.DistributorPartner, "South")
    ];

    public IReadOnlyList<Product> Products { get; } =
    [
        new("PROD-001", "Sample Product A", "General Supplies", 24.50m),
        new("PROD-002", "Sample Product B", "General Supplies", 49.00m),
        new("PROD-003", "Sample Product C", "Equipment", 125.00m)
    ];

    public IReadOnlyList<InventoryItem> Inventory { get; } =
    [
        new("PROD-001", "WH-NORTH", 500),
        new("PROD-002", "WH-WEST", 250),
        new("PROD-003", "WH-SOUTH", 90)
    ];

    public ApiClient? FindClient(string apiKey) =>
        ApiClients.FirstOrDefault(client => string.Equals(client.ApiKey, apiKey, StringComparison.Ordinal));

    public IReadOnlyList<PartnerOrder> GetOrdersForPartner(string partnerId)
    {
        lock (_ordersLock)
        {
            return _orders.Where(order => order.PartnerId == partnerId).ToList();
        }
    }

    public PartnerOrder CreateOrder(string partnerId, CreateOrderRequest request)
    {
        lock (_ordersLock)
        {
            var order = new PartnerOrder(
                $"ORD-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
                partnerId,
                request.ProductId,
                request.Quantity,
                DateOnly.FromDateTime(DateTime.UtcNow),
                "Submitted");

            _orders.Add(order);
            return order;
        }
    }

    public IReadOnlyList<SalesSummary> GetSalesSummaries()
    {
        lock (_ordersLock)
        {
            return _orders
                .GroupBy(order => order.ProductId)
                .Join(
                    Products,
                    group => group.Key,
                    product => product.Id,
                    (group, product) => new SalesSummary(
                        product.Id,
                        product.Name,
                        group.Sum(order => order.Quantity),
                        group.Sum(order => order.Quantity) * product.UnitPrice))
                .OrderBy(summary => summary.ProductId)
                .ToList();
        }
    }
}
