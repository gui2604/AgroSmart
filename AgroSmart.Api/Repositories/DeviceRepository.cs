using Microsoft.EntityFrameworkCore;
using AgroSmart.Api.Data;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Repositories;

public interface IDeviceRepository : IRepository<Device>
{
    Task<Device?> GetByIdentifierAsync(string identifier, bool tracking = false);
    Task<Device?> GetWithRegionAsync(int id);
    Task<List<Device>> GetByRegionAsync(int regionId);
    Task<List<Device>> GetAllWithRegionAsync();
}

public class DeviceRepository : Repository<Device>, IDeviceRepository
{
    public DeviceRepository(ApplicationDbContext context) : base(context) { }

    public Task<Device?> GetByIdentifierAsync(string identifier, bool tracking = false)
    {
        var query = tracking ? Set : Set.AsNoTracking();
        return query.FirstOrDefaultAsync(d => d.Identifier == identifier);
    }

    public Task<Device?> GetWithRegionAsync(int id) =>
        Set.AsNoTracking().Include(d => d.Region).FirstOrDefaultAsync(d => d.Id == id);

    public Task<List<Device>> GetByRegionAsync(int regionId) =>
        Set.AsNoTracking().Include(d => d.Region).Where(d => d.RegionId == regionId).ToListAsync();

    public Task<List<Device>> GetAllWithRegionAsync() =>
        Set.AsNoTracking().Include(d => d.Region).OrderBy(d => d.Identifier).ToListAsync();
}
