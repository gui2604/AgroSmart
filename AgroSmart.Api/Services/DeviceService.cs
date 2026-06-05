using AgroSmart.Api.Dtos;
using AgroSmart.Api.Exceptions;
using AgroSmart.Api.Models;
using AgroSmart.Api.Repositories;

namespace AgroSmart.Api.Services;

public interface IDeviceService
{
    Task<List<DeviceResponseDto>> GetAllAsync();
    Task<DeviceResponseDto> GetByIdAsync(int id);
    Task<List<DeviceResponseDto>> GetByRegionAsync(int regionId);
    Task<DeviceResponseDto> CreateAsync(DeviceCreateDto dto);
    Task<DeviceResponseDto> UpdateAsync(int id, DeviceUpdateDto dto);
    Task DeleteAsync(int id);
}

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _devices;
    private readonly IRegionRepository _regions;

    public DeviceService(IDeviceRepository devices, IRegionRepository regions)
    {
        _devices = devices;
        _regions = regions;
    }

    public async Task<List<DeviceResponseDto>> GetAllAsync()
    {
        var devices = await _devices.GetAllWithRegionAsync();
        return devices.Select(d => d.ToDto()).ToList();
    }

    public async Task<DeviceResponseDto> GetByIdAsync(int id)
    {
        var device = await _devices.GetWithRegionAsync(id) ?? throw NotFoundException.For("Dispositivo", id);
        return device.ToDto();
    }

    public async Task<List<DeviceResponseDto>> GetByRegionAsync(int regionId)
    {
        if (!await _regions.ExistsAsync(r => r.Id == regionId))
            throw NotFoundException.For("Região", regionId);

        var devices = await _devices.GetByRegionAsync(regionId);
        return devices.Select(d => d.ToDto()).ToList();
    }

    public async Task<DeviceResponseDto> CreateAsync(DeviceCreateDto dto)
    {
        if (!await _regions.ExistsAsync(r => r.Id == dto.RegionId))
            throw NotFoundException.For("Região", dto.RegionId);

        var identifier = dto.Identifier.Trim();
        if (await _devices.ExistsAsync(d => d.Identifier == identifier))
            throw new ValidationException($"Já existe um dispositivo com o identificador '{identifier}'.");

        var device = new Device
        {
            Identifier = identifier,
            Name = dto.Name,
            DeviceType = dto.DeviceType,
            FirmwareVersion = dto.FirmwareVersion,
            Status = dto.Status,
            RegionId = dto.RegionId
        };

        await _devices.AddAsync(device);
        await _devices.SaveChangesAsync();

        var created = await _devices.GetWithRegionAsync(device.Id);
        return created!.ToDto();
    }

    public async Task<DeviceResponseDto> UpdateAsync(int id, DeviceUpdateDto dto)
    {
        var device = await _devices.GetByIdAsync(id) ?? throw NotFoundException.For("Dispositivo", id);

        if (!await _regions.ExistsAsync(r => r.Id == dto.RegionId))
            throw NotFoundException.For("Região", dto.RegionId);

        device.Name = dto.Name;
        device.DeviceType = dto.DeviceType;
        device.FirmwareVersion = dto.FirmwareVersion;
        device.Status = dto.Status;
        device.RegionId = dto.RegionId;

        _devices.Update(device);
        await _devices.SaveChangesAsync();

        var updated = await _devices.GetWithRegionAsync(device.Id);
        return updated!.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var device = await _devices.GetByIdAsync(id) ?? throw NotFoundException.For("Dispositivo", id);
        _devices.Remove(device);
        await _devices.SaveChangesAsync();
    }
}
