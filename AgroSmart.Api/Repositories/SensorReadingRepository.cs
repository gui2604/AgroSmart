using Microsoft.EntityFrameworkCore;
using AgroSmart.Api.Data;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Repositories;

public interface ISensorReadingRepository : IRepository<SensorReading>
{
    Task<SensorReading?> GetWithMeasurementsAsync(int id);
    Task<List<SensorReading>> GetByDeviceAsync(int deviceId, int take = 50);
}

public class SensorReadingRepository : Repository<SensorReading>, ISensorReadingRepository
{
    public SensorReadingRepository(ApplicationDbContext context) : base(context) { }

    public Task<SensorReading?> GetWithMeasurementsAsync(int id) =>
        Set.AsNoTracking()
            .Include(r => r.Device)
            .Include(r => r.Measurements).ThenInclude(m => m.MetricType)
            .FirstOrDefaultAsync(r => r.Id == id);

    public Task<List<SensorReading>> GetByDeviceAsync(int deviceId, int take = 50) =>
        Set.AsNoTracking()
            .Include(r => r.Device)
            .Include(r => r.Measurements).ThenInclude(m => m.MetricType)
            .Where(r => r.DeviceId == deviceId)
            .OrderByDescending(r => r.CollectedAt)
            .Take(take)
            .ToListAsync();
}
