using MottuApi.Models;
using MottuApi.Services;

namespace MottuApi.Tests.Services
{
    public class PatioPrevisaoServiceTests
    {
        private readonly PatioPrevisaoService _service;

        public PatioPrevisaoServiceTests()
        {
            _service = new PatioPrevisaoService();
        }

        [Fact]
        public void PreverOcupacao_ComDadosValidos_DeveRetornarPrevisao()
        {
            // Arrange
            var request = new PrevisaoOcupacaoRequest
            {
                DiaDaSemana = 1, // Segunda-feira
                Hora = 12,
                MesDoAno = 1
            };

            // Act
            var resultado = _service.PreverOcupacao(request);

            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.NumeroFuncionariosPrevisto >= 0);
            Assert.NotEmpty(resultado.Periodo);
            Assert.NotEmpty(resultado.Recomendacao);
        }

        [Theory]
        [InlineData(1, 8, 1)]  // Segunda-feira manhã
        [InlineData(3, 12, 1)] // Quarta-feira tarde
        [InlineData(5, 18, 1)] // Sexta-feira noite
        [InlineData(0, 10, 1)] // Domingo manhã
        public void PreverOcupacao_ComDiferentesHorarios_DeveRetornarPrevisoes(int dia, int hora, int mes)
        {
            // Arrange
            var request = new PrevisaoOcupacaoRequest
            {
                DiaDaSemana = dia,
                Hora = hora,
                MesDoAno = mes
            };

            // Act
            var resultado = _service.PreverOcupacao(request);

            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.NumeroFuncionariosPrevisto >= 0);
        }

        [Theory]
        [InlineData(8, "Manhã")]
        [InlineData(14, "Tarde")]
        [InlineData(20, "Noite")]
        [InlineData(2, "Madrugada")]
        public void PreverOcupacao_DeveIdentificarPeriodoCorreto(int hora, string periodoEsperado)
        {
            // Arrange
            var request = new PrevisaoOcupacaoRequest
            {
                DiaDaSemana = 1,
                Hora = hora,
                MesDoAno = 1
            };

            // Act
            var resultado = _service.PreverOcupacao(request);

            // Assert
            Assert.Equal(periodoEsperado, resultado.Periodo);
        }

        [Theory]
        [InlineData(1)] // Segunda
        [InlineData(3)] // Quarta
        [InlineData(5)] // Sexta
        public void PreverOcupacao_DiasSemana_DevePreverMaisMovimento(int diaSemana)
        {
            // Arrange
            var request = new PrevisaoOcupacaoRequest
            {
                DiaDaSemana = diaSemana,
                Hora = 12,
                MesDoAno = 1
            };

            // Act
            var resultado = _service.PreverOcupacao(request);

            // Assert
            Assert.True(resultado.NumeroFuncionariosPrevisto > 20,
                $"Dia {diaSemana} deveria prever mais que 20 funcionários no horário de almoço");
        }

        [Theory]
        [InlineData(0)] // Domingo
        [InlineData(6)] // Sábado
        public void PreverOcupacao_FimDeSemana_DevePreverMenosMovimento(int diaSemana)
        {
            // Arrange
            var request = new PrevisaoOcupacaoRequest
            {
                DiaDaSemana = diaSemana,
                Hora = 12,
                MesDoAno = 1
            };

            // Act
            var resultado = _service.PreverOcupacao(request);

            // Assert
            Assert.True(resultado.NumeroFuncionariosPrevisto < 30,
                $"Fim de semana deveria prever menos movimento");
        }

        [Fact]
        public void PreverOcupacao_DeveIncluirRecomendacao()
        {
            // Arrange
            var request = new PrevisaoOcupacaoRequest
            {
                DiaDaSemana = 1,
                Hora = 12,
                MesDoAno = 1
            };

            // Act
            var resultado = _service.PreverOcupacao(request);

            // Assert
            Assert.NotNull(resultado.Recomendacao);
            Assert.NotEmpty(resultado.Recomendacao);
            Assert.Contains("movimento", resultado.Recomendacao.ToLower());
        }
    }
}
