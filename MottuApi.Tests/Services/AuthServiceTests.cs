using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using MottuApi.Data;
using MottuApi.Models;
using MottuApi.Services;

namespace MottuApi.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly MottuDbContext _context;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            // Setup InMemory Database
            var options = new DbContextOptionsBuilder<MottuDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MottuDbContext(options);

            // Setup Configuration Mock
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["Jwt:Key"]).Returns("MottuApiSecretKeyForTesting12345678!@#$%");
            _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("MottuApiTest");
            _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("MottuApiTestUsers");

            _authService = new AuthService(_context, _mockConfiguration.Object);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var patio = new Patio { Id = 1, Nome = "Pátio Teste", Endereco = "Rua Teste" };
            _context.Patios.Add(patio);

            var funcionario = new Funcionario
            {
                Id = 1,
                Nome = "Teste User",
                Email = "teste@mottu.com",
                Senha = _authService.HashPassword("senha123"),
                PatioId = 1
            };
            _context.Funcionarios.Add(funcionario);

            _context.SaveChanges();
        }

        [Fact]
        public async Task LoginAsync_ComCredenciaisValidas_DeveRetornarSucesso()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "teste@mottu.com",
                Senha = "senha123"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Token);
            Assert.NotEmpty(result.Token);
            Assert.NotNull(result.Funcionario);
            Assert.Equal("teste@mottu.com", result.Funcionario.Email);
        }

        [Fact]
        public async Task LoginAsync_ComEmailInvalido_DeveRetornarFalha()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "invalido@mottu.com",
                Senha = "senha123"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal("Email ou senha inválidos", result.Message);
        }

        [Fact]
        public async Task LoginAsync_ComSenhaInvalida_DeveRetornarFalha()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "teste@mottu.com",
                Senha = "senhaerrada"
            };

            // Act
            var result = await _authService.LoginAsync(loginRequest);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Token);
            Assert.Equal("Email ou senha inválidos", result.Message);
        }

        [Fact]
        public void HashPassword_DeveCriarHashConsistente()
        {
            // Arrange
            var senha = "minhasenha123";

            // Act
            var hash1 = _authService.HashPassword(senha);
            var hash2 = _authService.HashPassword(senha);

            // Assert
            Assert.NotEmpty(hash1);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void HashPassword_SenhasDiferentes_DevemGerarHashesDiferentes()
        {
            // Arrange
            var senha1 = "senha1";
            var senha2 = "senha2";

            // Act
            var hash1 = _authService.HashPassword(senha1);
            var hash2 = _authService.HashPassword(senha2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }
    }
}
