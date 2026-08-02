using Microsoft.AspNetCore.Mvc;
using PartnerDataSharing.Api.Services;

namespace PartnerDataSharing.Api.Controllers;

[ApiController]
[Route("api/compliance")]
public sealed class ComplianceController : ControllerBase
{
    private readonly AuditEventStore _auditEventStore;

    public ComplianceController(AuditEventStore auditEventStore)
    {
        _auditEventStore = auditEventStore;
    }

    [HttpGet("audit-events")]
    public IActionResult GetAuditEvents() => Ok(_auditEventStore.GetAll());
}
