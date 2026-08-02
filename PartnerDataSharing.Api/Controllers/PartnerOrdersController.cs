using Microsoft.AspNetCore.Mvc;
using PartnerDataSharing.Api.Domain;
using PartnerDataSharing.Api.Services;

namespace PartnerDataSharing.Api.Controllers;

[ApiController]
[Route("api/partners/{partnerId}/orders")]
public sealed class PartnerOrdersController : ControllerBase
{
    private readonly DemoDataStore _dataStore;

    public PartnerOrdersController(DemoDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    [HttpGet]
    public IActionResult GetOrders(string partnerId) => Ok(_dataStore.GetOrdersForPartner(partnerId));

    [HttpPost]
    public IActionResult CreateOrder(string partnerId, CreateOrderRequest request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest(new { error = "Quantity must be greater than zero." });
        }

        if (_dataStore.Products.All(product => product.Id != request.ProductId))
        {
            return BadRequest(new { error = "Unknown product." });
        }

        var order = _dataStore.CreateOrder(partnerId, request);
        return CreatedAtAction(nameof(GetOrders), new { partnerId }, order);
    }
}
