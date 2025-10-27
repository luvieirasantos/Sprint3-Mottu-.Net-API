using Microsoft.AspNetCore.Mvc;
using MottuApi.Models;
using MottuApi.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

namespace MottuApi.Controllers
{
    /// <summary>
    /// Controller para previsões usando ML.NET
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PrevisaoController : ControllerBase
    {
        private readonly PatioPrevisaoService _previsaoService;

        public PrevisaoController(PatioPrevisaoService previsaoService)
        {
            _previsaoService = previsaoService;
        }

        /// <summary>
        /// Prevê a ocupação de funcionários em um pátio usando ML.NET
        /// </summary>
        /// <param name="request">Dados para previsão (dia da semana, hora e mês)</param>
        /// <returns>Previsão de número de funcionários e recomendação</returns>
        /// <response code="200">Previsão realizada com sucesso</response>
        /// <response code="400">Dados de entrada inválidos</response>
        [HttpPost("ocupacao-patio")]
        public ActionResult<PrevisaoOcupacaoResponse> PreverOcupacaoPatio([FromBody] PrevisaoOcupacaoRequest request)
        {
            // Validar entrada
            if (request.DiaDaSemana < 0 || request.DiaDaSemana > 6)
            {
                return BadRequest("Dia da semana deve estar entre 0 (Domingo) e 6 (Sábado)");
            }

            if (request.Hora < 0 || request.Hora > 23)
            {
                return BadRequest("Hora deve estar entre 0 e 23");
            }

            if (request.MesDoAno < 1 || request.MesDoAno > 12)
            {
                return BadRequest("Mês deve estar entre 1 e 12");
            }

            try
            {
                var previsao = _previsaoService.PreverOcupacao(request);

                var result = new
                {
                    Data = previsao,
                    Links = new
                    {
                        Self = Url.Action("PreverOcupacaoPatio"),
                        Documentation = "/swagger"
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao gerar previsão: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém informações sobre o modelo de ML
        /// </summary>
        /// <returns>Informações sobre o modelo de previsão</returns>
        /// <response code="200">Informações recuperadas com sucesso</response>
        [HttpGet("info")]
        public ActionResult<object> GetModeloInfo()
        {
            var info = new
            {
                Nome = "Modelo de Previsão de Ocupação de Pátios",
                Versao = "1.0",
                Algoritmo = "SDCA (Stochastic Dual Coordinate Ascent)",
                Framework = "ML.NET",
                Descricao = "Modelo de regressão para prever o número de funcionários necessários em um pátio com base no dia da semana, hora e mês",
                Parametros = new
                {
                    DiaDaSemana = "0-6 (Domingo-Sábado)",
                    Hora = "0-23",
                    MesDoAno = "1-12"
                },
                Links = new
                {
                    Self = Url.Action("GetModeloInfo"),
                    Prever = Url.Action("PreverOcupacaoPatio")
                }
            };

            return Ok(info);
        }
    }
}
