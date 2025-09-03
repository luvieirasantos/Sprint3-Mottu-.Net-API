using System.ComponentModel.DataAnnotations;

namespace MottuApi.Models
{
    public class Patio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [StringLength(200)]
        public string Endereco { get; set; }

        // Relacionamento com Gerente
        public int? GerenteId { get; set; }
        public Gerente? Gerente { get; set; }
    }
}