using Microsoft.ML;
using Microsoft.ML.Data;
using MottuApi.Models;

namespace MottuApi.Services
{
    /// <summary>
    /// Serviço para previsão de ocupação de pátios usando ML.NET
    /// </summary>
    public class PatioPrevisaoService
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;

        public PatioPrevisaoService()
        {
            _mlContext = new MLContext(seed: 0);
            TreinarModelo();
        }

        /// <summary>
        /// Treina o modelo de ML com dados de exemplo
        /// </summary>
        private void TreinarModelo()
        {
            // Dados de treinamento simulados (em produção, viriam de um banco de dados)
            var dadosTreinamento = new List<PatioOcupacaoData>
            {
                // Segunda-feira
                new() { DiaDaSemana = 1, Hora = 8, MesDoAno = 1, NumeroFuncionarios = 25 },
                new() { DiaDaSemana = 1, Hora = 12, MesDoAno = 1, NumeroFuncionarios = 40 },
                new() { DiaDaSemana = 1, Hora = 18, MesDoAno = 1, NumeroFuncionarios = 30 },

                // Terça-feira
                new() { DiaDaSemana = 2, Hora = 8, MesDoAno = 1, NumeroFuncionarios = 28 },
                new() { DiaDaSemana = 2, Hora = 12, MesDoAno = 1, NumeroFuncionarios = 42 },
                new() { DiaDaSemana = 2, Hora = 18, MesDoAno = 1, NumeroFuncionarios = 32 },

                // Quarta-feira
                new() { DiaDaSemana = 3, Hora = 8, MesDoAno = 1, NumeroFuncionarios = 30 },
                new() { DiaDaSemana = 3, Hora = 12, MesDoAno = 1, NumeroFuncionarios = 45 },
                new() { DiaDaSemana = 3, Hora = 18, MesDoAno = 1, NumeroFuncionarios = 35 },

                // Quinta-feira
                new() { DiaDaSemana = 4, Hora = 8, MesDoAno = 1, NumeroFuncionarios = 27 },
                new() { DiaDaSemana = 4, Hora = 12, MesDoAno = 1, NumeroFuncionarios = 43 },
                new() { DiaDaSemana = 4, Hora = 18, MesDoAno = 1, NumeroFuncionarios = 33 },

                // Sexta-feira
                new() { DiaDaSemana = 5, Hora = 8, MesDoAno = 1, NumeroFuncionarios = 32 },
                new() { DiaDaSemana = 5, Hora = 12, MesDoAno = 1, NumeroFuncionarios = 48 },
                new() { DiaDaSemana = 5, Hora = 18, MesDoAno = 1, NumeroFuncionarios = 38 },

                // Sábado
                new() { DiaDaSemana = 6, Hora = 8, MesDoAno = 1, NumeroFuncionarios = 15 },
                new() { DiaDaSemana = 6, Hora = 12, MesDoAno = 1, NumeroFuncionarios = 25 },
                new() { DiaDaSemana = 6, Hora = 18, MesDoAno = 1, NumeroFuncionarios = 18 },

                // Domingo
                new() { DiaDaSemana = 0, Hora = 8, MesDoAno = 1, NumeroFuncionarios = 10 },
                new() { DiaDaSemana = 0, Hora = 12, MesDoAno = 1, NumeroFuncionarios = 15 },
                new() { DiaDaSemana = 0, Hora = 18, MesDoAno = 1, NumeroFuncionarios = 12 },
            };

            // Carregar dados
            var dataView = _mlContext.Data.LoadFromEnumerable(dadosTreinamento);

            // Definir pipeline de treinamento
            var pipeline = _mlContext.Transforms.Concatenate("Features",
                    nameof(PatioOcupacaoData.DiaDaSemana),
                    nameof(PatioOcupacaoData.Hora),
                    nameof(PatioOcupacaoData.MesDoAno))
                .Append(_mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", maximumNumberOfIterations: 100));

            // Treinar modelo
            _model = pipeline.Fit(dataView);
        }

        /// <summary>
        /// Faz uma previsão de ocupação baseada nos parâmetros fornecidos
        /// </summary>
        public PrevisaoOcupacaoResponse PreverOcupacao(PrevisaoOcupacaoRequest request)
        {
            if (_model == null)
            {
                throw new InvalidOperationException("Modelo não treinado");
            }

            // Criar engine de previsão
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<PatioOcupacaoData, PatioOcupacaoPredicao>(_model);

            // Fazer previsão
            var input = new PatioOcupacaoData
            {
                DiaDaSemana = request.DiaDaSemana,
                Hora = request.Hora,
                MesDoAno = request.MesDoAno
            };

            var resultado = predictionEngine.Predict(input);
            var numeroFuncionariosPrevisto = (int)Math.Round(resultado.NumeroFuncionariosPrevisto);

            // Garantir valor mínimo
            if (numeroFuncionariosPrevisto < 0)
            {
                numeroFuncionariosPrevisto = 5;
            }

            // Determinar período e recomendação
            var periodo = DeterminarPeriodo(request.Hora);
            var recomendacao = GerarRecomendacao(numeroFuncionariosPrevisto, request.DiaDaSemana);

            return new PrevisaoOcupacaoResponse
            {
                NumeroFuncionariosPrevisto = numeroFuncionariosPrevisto,
                Periodo = periodo,
                Recomendacao = recomendacao
            };
        }

        private string DeterminarPeriodo(int hora)
        {
            return hora switch
            {
                >= 6 and < 12 => "Manhã",
                >= 12 and < 18 => "Tarde",
                >= 18 and < 24 => "Noite",
                _ => "Madrugada"
            };
        }

        private string GerarRecomendacao(int numeroFuncionarios, int diaSemana)
        {
            var diaSemanaTexto = diaSemana switch
            {
                0 => "Domingo",
                1 => "Segunda-feira",
                2 => "Terça-feira",
                3 => "Quarta-feira",
                4 => "Quinta-feira",
                5 => "Sexta-feira",
                6 => "Sábado",
                _ => "Dia inválido"
            };

            if (numeroFuncionarios >= 40)
            {
                return $"Alto movimento previsto para {diaSemanaTexto}. Recomenda-se escala completa de funcionários.";
            }
            else if (numeroFuncionarios >= 25)
            {
                return $"Movimento moderado previsto para {diaSemanaTexto}. Escala padrão recomendada.";
            }
            else
            {
                return $"Movimento baixo previsto para {diaSemanaTexto}. Escala reduzida pode ser suficiente.";
            }
        }
    }
}
