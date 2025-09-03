using System.ComponentModel.DataAnnotations;

namespace MottuApi.Models
{
    public class Gerente
    {
        [Key]
        public int Id { get; set; }

        // Relacionamento com Funcionario
        public int FuncionarioId { get; set; }
        public Funcionario? Funcionario { get; set; }

        // Relacionamento com Patio
        public int PatioId { get; set; }
        public Patio? Patio { get; set; }
    }
}