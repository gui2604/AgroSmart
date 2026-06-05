using Microsoft.EntityFrameworkCore;
using AgroSmart.Api.Data;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Repositories;

public interface IAlertRuleRepository : IRepository<AlertRule>
{
    /// <summary>
    /// Returns the active rules that apply to a given metric in a given region
    /// (region-specific rules plus global rules where RegionId is null).
    /// </summary>
    Task<List<AlertRule>> GetActiveRulesAsync(int metricTypeId, int regionId);

    Task<AlertRule?> GetWithDetailsAsync(int id);
    Task<List<AlertRule>> GetAllWithDetailsAsync();
}

public class AlertRuleRepository : Repository<AlertRule>, IAlertRuleRepository
{
    public AlertRuleRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<AlertRule>> GetActiveRulesAsync(int metricTypeId, int regionId)
    {
        // Filter IsActive in memory: Oracle EF provider mishandles bool in SQL for NUMBER(1) columns.
        var rules = await Set.AsNoTracking()
            .Where(r => r.MetricTypeId == metricTypeId
                        && (r.RegionId == null || r.RegionId == regionId))
            .ToListAsync();

        return rules.Where(r => r.IsActive).ToList();
    }

    public Task<AlertRule?> GetWithDetailsAsync(int id) =>
        Set.AsNoTracking()
            .Include(r => r.MetricType)
            .Include(r => r.Region)
            .FirstOrDefaultAsync(r => r.Id == id);

    public Task<List<AlertRule>> GetAllWithDetailsAsync() =>
        Set.AsNoTracking()
            .Include(r => r.MetricType)
            .Include(r => r.Region)
            .OrderBy(r => r.Id)
            .ToListAsync();
}
