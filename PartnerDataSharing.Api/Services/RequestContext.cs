using PartnerDataSharing.Api.Domain;

namespace PartnerDataSharing.Api.Services;

public sealed class RequestContext
{
    public string CorrelationId { get; set; } = string.Empty;

    public ApiClient? Client { get; set; }

    public string? PartnerId => Client?.PartnerId;

    public PartnerRole? Role => Client?.Role;
}
