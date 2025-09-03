using MottuApi.Models;

namespace MottuApi.Tests;

public class UnitTest1
{
    [Fact]
    public void Patio_DeveSerCriadoComPropriedadesValidas()
    {
        // Arrange
        var patio = new Patio
        {
            Nome = "Pátio Central",
            Endereco = "Rua Principal, 123"
        };

        // Act & Assert
        Assert.Equal("Pátio Central", patio.Nome);
        Assert.Equal("Rua Principal, 123", patio.Endereco);
        Assert.Null(patio.GerenteId);
    }

    [Fact]
    public void Funcionario_DeveSerCriadoComPropriedadesValidas()
    {
        // Arrange
        var funcionario = new Funcionario
        {
            Nome = "João Silva",
            Email = "joao@mottu.com",
            Senha = "senha123",
            PatioId = 1
        };

        // Act & Assert
        Assert.Equal("João Silva", funcionario.Nome);
        Assert.Equal("joao@mottu.com", funcionario.Email);
        Assert.Equal("senha123", funcionario.Senha);
        Assert.Equal(1, funcionario.PatioId);
    }

    [Fact]
    public void Gerente_DeveSerCriadoComPropriedadesValidas()
    {
        // Arrange
        var gerente = new Gerente
        {
            FuncionarioId = 1,
            PatioId = 1
        };

        // Act & Assert
        Assert.Equal(1, gerente.FuncionarioId);
        Assert.Equal(1, gerente.PatioId);
    }
}
