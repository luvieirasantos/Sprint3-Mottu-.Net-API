using Microsoft.ML.Data;

namespace MottuApi.Models
{
    /// <summary>
    /// Dados de entrada para previsão de ocupação de pátio
    /// </summary>
    public class PatioOcupacaoData
    {
        [LoadColumn(0)]
        public float DiaDaSemana { get; set; }

        [LoadColumn(1)]
        public float Hora { get; set; }

        [LoadColumn(2)]
        public float MesDoAno { get; set; }

        [LoadColumn(3)]
        [ColumnName("Label")]
        public float NumeroFuncionarios { get; set; }
    }

    /// <summary>
    /// Resultado da previsão de ocupação
    /// </summary>
    public class PatioOcupacaoPredicao
    {
        [ColumnName("Score")]
        public float NumeroFuncionariosPrevisto { get; set; }
    }

    /// <summary>
    /// Request para previsão de ocupação
    /// </summary>
    public class PrevisaoOcupacaoRequest
    {
        public int DiaDaSemana { get; set; } // 0-6 (Domingo-Sábado)
        public int Hora { get; set; } // 0-23
        public int MesDoAno { get; set; } // 1-12
    }

    /// <summary>
    /// Response da previsão de ocupação
    /// </summary>
    public class PrevisaoOcupacaoResponse
    {
        public int NumeroFuncionariosPrevisto { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public string Recomendacao { get; set; } = string.Empty;
    }
}
