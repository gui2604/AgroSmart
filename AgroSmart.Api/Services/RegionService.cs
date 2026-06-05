using AgroSmart.Api.Dtos;
using AgroSmart.Api.Exceptions;
using AgroSmart.Api.Models;
using AgroSmart.Api.Repositories;

namespace AgroSmart.Api.Services;

public interface IRegionService
{
    Task<List<RegionResponseDto>> GetAllAsync();
    Task<RegionResponseDto> GetByIdAsync(int id);
    Task<RegionResponseDto> CreateAsync(RegionCreateDto dto);
    Task<RegionResponseDto> UpdateAsync(int id, RegionUpdateDto dto);
    Task DeleteAsync(int id);
}

public class RegionService : IRegionService
{
    private readonly IRegionRepository _repo;

    public RegionService(IRegionRepository repo) => _repo = repo;

    public async Task<List<RegionResponseDto>> GetAllAsync()
    {
        var regions = await _repo.GetAllWithDeviceCountAsync();
        return regions.Select(r => r.ToDto()).ToList();
    }

    public async Task<RegionResponseDto> GetByIdAsync(int id)
    {
        var region = await _repo.GetByIdAsync(id) ?? throw NotFoundException.For("Região", id);
        return region.ToDto();
    }

    public async Task<RegionResponseDto> CreateAsync(RegionCreateDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _repo.ExistsAsync(r => r.Code == code))
            throw new ValidationException($"Já existe uma região com o código '{code}'.");

        var region = new Region
        {
            Code = code,
            Name = dto.Name,
            ModuleType = dto.ModuleType,
            FieldLocation = dto.FieldLocation,
            Description = dto.Description
        };

        await _repo.AddAsync(region);
        await _repo.SaveChangesAsync();
        return region.ToDto();
    }

    public async Task<RegionResponseDto> UpdateAsync(int id, RegionUpdateDto dto)
    {
        var region = await _repo.GetByIdAsync(id) ?? throw NotFoundException.For("Região", id);

        region.Name = dto.Name;
        region.ModuleType = dto.ModuleType;
        region.FieldLocation = dto.FieldLocation;
        region.Description = dto.Description;

        _repo.Update(region);
        await _repo.SaveChangesAsync();
        return region.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var region = await _repo.GetByIdAsync(id) ?? throw NotFoundException.For("Região", id);

        if (await _repo.ExistsAsync(r => r.Id == id && r.Devices.Any()))
            throw new ValidationException("Não é possível excluir uma região que possui dispositivos vinculados.");

        _repo.Remove(region);
        await _repo.SaveChangesAsync();
    }
}
