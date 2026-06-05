using Microsoft.EntityFrameworkCore;
using AgroSmart.Api.Data;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Repositories;

public interface IRegionRepository : IRepository<Region>
{
    Task<Region?> GetByCodeAsync(string code);
    Task<List<Region>> GetAllWithDeviceCountAsync();
}

public class RegionRepository : Repository<Region>, IRegionRepository
{
    public RegionRepository(ApplicationDbContext context) : base(context) { }

    public Task<Region?> GetByCodeAsync(string code) =>
        Set.AsNoTracking().FirstOrDefaultAsync(r => r.Code == code);

    public Task<List<Region>> GetAllWithDeviceCountAsync() =>
        Set.AsNoTracking().Include(r => r.Devices).OrderBy(r => r.Code).ToListAsync();
}
