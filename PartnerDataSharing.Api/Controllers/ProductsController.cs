using Microsoft.AspNetCore.Mvc;
using PartnerDataSharing.Api.Services;

namespace PartnerDataSharing.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly DemoDataStore _dataStore;

    public ProductsController(DemoDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    [HttpGet]
    public IActionResult GetProducts() => Ok(_dataStore.Products);
}
