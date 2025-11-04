using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using MottuApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.TestHost;

namespace MottuApi.Tests.Integration
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Desabilitar autenticação para testes
                    services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
                });
            }).CreateClient();
        }

        [Fact]
        public async Task HealthCheck_DeveRetornarHealthy()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetPatios_DeveRetornarSucesso()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/patios");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetFuncionarios_DeveRetornarSucesso()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/funcionarios");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetGerentes_DeveRetornarSucesso()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/gerentes");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PrevisaoInfo_DeveRetornarInformacoesDoModelo()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/previsao/info");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("ML.NET", content);
        }

        [Fact]
        public async Task PrevisaoOcupacao_ComDadosValidos_DeveRetornarPrevisao()
        {
            // Arrange
            var request = new PrevisaoOcupacaoRequest
            {
                DiaDaSemana = 1,
                Hora = 12,
                MesDoAno = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/previsao/ocupacao-patio", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("numeroFuncionariosPrevisto", content);
        }

        [Fact]
        public async Task PrevisaoOcupacao_ComDiaInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            var request = new PrevisaoOcupacaoRequest
            {
                DiaDaSemana = 10, // Inválido
                Hora = 12,
                MesDoAno = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/previsao/ocupacao-patio", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "joao.silva@mottu.com",
                Senha = "123456"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("token", content);
        }

        [Fact]
        public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "invalido@mottu.com",
                Senha = "senhaerrada"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Swagger_DeveEstarDisponivel()
        {
            // Act
            var response = await _client.GetAsync("/swagger");

            // Assert
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.MovedPermanently);
        }
    }

    public class FakePolicyEvaluator : IPolicyEvaluator
    {
        public Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> AuthenticateAsync(AuthorizationPolicy policy, Microsoft.AspNetCore.Http.HttpContext context)
        {
            var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Test user") };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, "Test");
            var result = Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket);
            return Task.FromResult(result);
        }

        public Task<PolicyAuthorizationResult> AuthorizeAsync(AuthorizationPolicy policy, Microsoft.AspNetCore.Authentication.AuthenticateResult authenticationResult, Microsoft.AspNetCore.Http.HttpContext context, object? resource)
        {
            return Task.FromResult(PolicyAuthorizationResult.Success());
        }
    }
}
