using System.ComponentModel.DataAnnotations;

namespace MottuApi.Models
{
    public class Funcionario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(256)] // Para hash de senha
        public string Senha { get; set; }

        // Relacionamento com Patio
        public int PatioId { get; set; }
        public Patio? Patio { get; set; }
    }
}