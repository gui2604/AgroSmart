using Moq;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Exceptions;
using AgroSmart.Api.Models;
using AgroSmart.Api.Repositories;
using AgroSmart.Api.Services;
using AgroSmart.Api.Tests.Support;
using Xunit;
using Xunit.Abstractions;

namespace AgroSmart.Api.Tests.Services;

public class RegionServiceTests
{
    private readonly TestScenarioLogger _log;

    public RegionServiceTests(ITestOutputHelper output) => _log = new TestScenarioLogger(output);

    [Fact(DisplayName = "CT-04 — Região com código duplicado é rejeitada")]
    public async Task CT04_DuplicateRegionCode_ThrowsValidation()
    {
        _log.Begin("CT-04", "Código de região duplicado — ValidationException");

        var repo = new Mock<IRegionRepository>();
        repo.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Region, bool>>>()))
            .ReturnsAsync(true);

        var service = new RegionService(repo.Object);
        var dto = new RegionCreateDto { Code = "BAY-A1", Name = "Baia A1" };

        _log.Data("Código", dto.Code);
        _log.Step("Simulando que BAY-A1 já existe no Oracle");

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(dto));

        _log.Data("Mensagem API", ex.Message);
        Assert.Contains("BAY-A1", ex.Message);
        _log.Pass("API retorna 400 — evita duplicidade no catálogo de regiões.");
    }

    [Fact(DisplayName = "CT-06 — Exclusão de região com dispositivos é bloqueada")]
    public async Task CT06_DeleteRegionWithDevices_ThrowsValidation()
    {
        _log.Begin("CT-06", "Excluir região com dispositivos — bloqueado");

        var region = new Region { Id = 3, Code = "BAY-B2", Name = "Baia B2" };
        var repo = new Mock<IRegionRepository>();
        repo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(region);
        repo.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Region, bool>>>()))
            .ReturnsAsync(true);

        var service = new RegionService(repo.Object);

        _log.Data("Região", $"{region.Code} (id={region.Id})");
        _log.Step("Simulando 2 dispositivos vinculados");

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.DeleteAsync(3));

        _log.Data("Mensagem API", ex.Message);
        Assert.Contains("dispositivos", ex.Message, StringComparison.OrdinalIgnoreCase);
        _log.Pass("Integridade referencial preservada; DELETE retorna 400.");
    }
}
