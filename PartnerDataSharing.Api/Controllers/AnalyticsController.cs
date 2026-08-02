using Microsoft.AspNetCore.Mvc;
using PartnerDataSharing.Api.Services;

namespace PartnerDataSharing.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly DemoDataStore _dataStore;

    public AnalyticsController(DemoDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    [HttpGet("sales-summary")]
    public IActionResult GetSalesSummary() => Ok(_dataStore.GetSalesSummaries());
}
