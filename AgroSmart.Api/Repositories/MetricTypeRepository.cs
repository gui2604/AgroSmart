using Microsoft.EntityFrameworkCore;
using AgroSmart.Api.Data;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Repositories;

public interface IMetricTypeRepository : IRepository<MetricType>
{
    Task<MetricType?> GetByCodeAsync(string code);
    Task<Dictionary<string, MetricType>> GetByCodesAsync(IEnumerable<string> codes);
}

public class MetricTypeRepository : Repository<MetricType>, IMetricTypeRepository
{
    public MetricTypeRepository(ApplicationDbContext context) : base(context) { }

    public Task<MetricType?> GetByCodeAsync(string code) =>
        Set.AsNoTracking().FirstOrDefaultAsync(m => m.Code == code);

    public async Task<Dictionary<string, MetricType>> GetByCodesAsync(IEnumerable<string> codes)
    {
        var distinct = codes.Select(c => c.ToUpperInvariant()).Distinct().ToList();
        var matches = await Set.AsNoTracking()
            .Where(m => distinct.Contains(m.Code))
            .ToListAsync();
        return matches.ToDictionary(m => m.Code, m => m);
    }
}
