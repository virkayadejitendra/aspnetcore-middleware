using Microsoft.AspNetCore.Mvc;
using PartnerDataSharing.Api.Services;

namespace PartnerDataSharing.Api.Controllers;

[ApiController]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly DemoDataStore _dataStore;

    public InventoryController(DemoDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    [HttpGet]
    public IActionResult GetInventory() => Ok(_dataStore.Inventory);
}
