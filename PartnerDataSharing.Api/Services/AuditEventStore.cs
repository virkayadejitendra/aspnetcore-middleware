using PartnerDataSharing.Api.Domain;

namespace PartnerDataSharing.Api.Services;

public sealed class AuditEventStore
{
    private readonly List<AuditEvent> _events = [];
    private readonly object _lock = new();

    public void Add(AuditEvent auditEvent)
    {
        lock (_lock)
        {
            _events.Add(auditEvent);
        }
    }

    public IReadOnlyList<AuditEvent> GetAll()
    {
        lock (_lock)
        {
            return _events.OrderByDescending(auditEvent => auditEvent.Timestamp).ToList();
        }
    }
}
