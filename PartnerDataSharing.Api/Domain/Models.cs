namespace PartnerDataSharing.Api.Domain;

public sealed record ApiClient(
    string ApiKey,
    string ClientName,
    PartnerRole Role,
    string? PartnerId);

public sealed record Partner(
    string Id,
    string Name,
    PartnerRole Role,
    string Region);

public sealed record Product(
    string Id,
    string Name,
    string Category,
    decimal UnitPrice);

public sealed record InventoryItem(
    string ProductId,
    string WarehouseCode,
    int AvailableUnits);

public sealed record PartnerOrder(
    string Id,
    string PartnerId,
    string ProductId,
    int Quantity,
    DateOnly OrderDate,
    string Status);

public sealed record CreateOrderRequest(
    string ProductId,
    int Quantity);

public sealed record SalesSummary(
    string ProductId,
    string ProductName,
    int TotalUnits,
    decimal TotalRevenue);

public sealed record AuditEvent(
    Guid Id,
    DateTimeOffset Timestamp,
    string CorrelationId,
    string ClientName,
    PartnerRole Role,
    string? PartnerId,
    string Method,
    string Path,
    int StatusCode,
    long DurationMs);
