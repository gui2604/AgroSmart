using Microsoft.Extensions.Configuration;
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

public class UserServiceTests
{
    private readonly TestScenarioLogger _log;

    public UserServiceTests(ITestOutputHelper output) => _log = new TestScenarioLogger(output);

    [Fact(DisplayName = "CT-05 — Login com e-mail inexistente retorna credenciais inválidas")]
    public async Task CT05_LoginUnknownEmail_ThrowsValidation()
    {
        _log.Begin("CT-05", "Login e-mail inexistente — 400");

        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByEmailAsync("desconhecido@fiap.test")).ReturnsAsync((User?)null);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "agrosmart-unit-test-secret-key-32chars!!",
                ["Jwt:Issuer"] = "agrosmart",
                ["Jwt:Audience"] = "agrosmart-clients"
            })
            .Build();

        var service = new UserService(repo.Object, config);
        var dto = new UserLoginDto { Email = "desconhecido@fiap.test", Password = "qualquer" };

        _log.Data("E-mail", dto.Email);
        _log.Step("Usuário não existe em AGS_USERS");

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.LoginAsync(dto));

        _log.Data("Mensagem API", ex.Message);
        Assert.Equal("Credenciais inválidas.", ex.Message);
        _log.Pass("Mesma mensagem vista no Swagger/Postman sem register prévio.");
    }
}
