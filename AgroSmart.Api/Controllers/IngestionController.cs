using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Exceptions;
using AgroSmart.Api.Services;

namespace AgroSmart.Api.Controllers;

/// <summary>
/// Receives environmental data transmitted by field devices. Simulates real sensor
/// telemetry through JSON payloads (single, batch, or uploaded file). Each ingested
/// reading is evaluated against the configured alert rules.
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/ingestion")]
[Produces("application/json")]
public class IngestionController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IIngestionService _service;

    public IngestionController(IIngestionService service) => _service = service;

    /// <summary>Ingests a single sensor reading payload.</summary>
    [HttpPost("readings")]
    public async Task<ActionResult<IngestionResultDto>> IngestSingle([FromBody] SensorReadingIngestDto dto)
        => Ok(await _service.IngestAsync(dto));

    /// <summary>Ingests a batch of sensor reading payloads.</summary>
    [HttpPost("readings/batch")]
    public async Task<ActionResult<BatchIngestionResultDto>> IngestBatch([FromBody] List<SensorReadingIngestDto> payloads)
    {
        if (payloads is null || payloads.Count == 0)
            throw new ValidationException("O lote não pode estar vazio.");
        return Ok(await _service.IngestBatchAsync(payloads));
    }

    /// <summary>
    /// Uploads a JSON file produced by a sensor. The file may contain a single reading
    /// object or an array of readings.
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<BatchIngestionResultDto>> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new ValidationException("Arquivo não enviado ou vazio.");

        using var stream = file.OpenReadStream();
        using var doc = await JsonDocument.ParseAsync(stream);

        var payloads = new List<SensorReadingIngestDto>();
        try
        {
            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                payloads = doc.RootElement.Deserialize<List<SensorReadingIngestDto>>(JsonOptions) ?? new();
            }
            else
            {
                var single = doc.RootElement.Deserialize<SensorReadingIngestDto>(JsonOptions);
                if (single is not null) payloads.Add(single);
            }
        }
        catch (JsonException ex)
        {
            throw new ValidationException($"JSON inválido: {ex.Message}");
        }

        if (payloads.Count == 0)
            throw new ValidationException("Nenhuma leitura encontrada no arquivo.");

        return Ok(await _service.IngestBatchAsync(payloads, file.FileName));
    }
}
