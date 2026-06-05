using AgroSmart.Api.Dtos;
using AgroSmart.Api.Exceptions;
using AgroSmart.Api.Repositories;

namespace AgroSmart.Api.Services;

public interface IReadingService
{
    Task<SensorReadingResponseDto> GetByIdAsync(int id);
    Task<List<SensorReadingResponseDto>> GetByDeviceAsync(int deviceId, int take);
}

public class ReadingService : IReadingService
{
    private readonly ISensorReadingRepository _readings;
    private readonly IDeviceRepository _devices;

    public ReadingService(ISensorReadingRepository readings, IDeviceRepository devices)
    {
        _readings = readings;
        _devices = devices;
    }

    public async Task<SensorReadingResponseDto> GetByIdAsync(int id)
    {
        var reading = await _readings.GetWithMeasurementsAsync(id) ?? throw NotFoundException.For("Leitura", id);
        return reading.ToDto();
    }

    public async Task<List<SensorReadingResponseDto>> GetByDeviceAsync(int deviceId, int take)
    {
        if (!await _devices.ExistsAsync(d => d.Id == deviceId))
            throw NotFoundException.For("Dispositivo", deviceId);

        var readings = await _readings.GetByDeviceAsync(deviceId, take <= 0 ? 50 : take);
        return readings.Select(r => r.ToDto()).ToList();
    }
}
