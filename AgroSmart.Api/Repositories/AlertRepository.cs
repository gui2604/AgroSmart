using Microsoft.EntityFrameworkCore;
using AgroSmart.Api.Data;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Repositories;

public interface IAlertRepository : IRepository<Alert>
{
    Task<List<Alert>> GetFilteredAsync(int? regionId, int? deviceId, AlertStatus? status, AlertSeverity? severity);
    Task<Alert?> GetWithDetailsAsync(int id);
}

public class AlertRepository : Repository<Alert>, IAlertRepository
{
    public AlertRepository(ApplicationDbContext context) : base(context) { }

    private IQueryable<Alert> WithDetails(bool tracking = false)
    {
        var query = tracking ? Set : Set.AsNoTracking();
        return query
            .Include(a => a.MetricType)
            .Include(a => a.Device)
            .Include(a => a.Region)
            .Include(a => a.AlertRule);
    }

    public Task<List<Alert>> GetFilteredAsync(int? regionId, int? deviceId, AlertStatus? status, AlertSeverity? severity)
    {
        var query = WithDetails();

        if (regionId.HasValue) query = query.Where(a => a.RegionId == regionId.Value);
        if (deviceId.HasValue) query = query.Where(a => a.DeviceId == deviceId.Value);
        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (severity.HasValue) query = query.Where(a => a.Severity == severity.Value);

        return query.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public Task<Alert?> GetWithDetailsAsync(int id) =>
        WithDetails(tracking: true).FirstOrDefaultAsync(a => a.Id == id);
}
