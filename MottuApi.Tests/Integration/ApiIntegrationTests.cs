using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using MottuApi.Models;

namespace MottuApi.Tests.Integration
{
    public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public ApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
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
            Assert.Contains("NumeroFuncionariosPrevisto", content);
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
                Email = "funcionario1@mottu.com",
                Senha = "senha123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Token", content);
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
}
