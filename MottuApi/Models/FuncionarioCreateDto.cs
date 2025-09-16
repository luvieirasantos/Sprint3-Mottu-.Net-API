using System.ComponentModel.DataAnnotations;

namespace MottuApi.Models
{
    public class FuncionarioCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Senha { get; set; } = string.Empty;

        [Required]
        public int PatioId { get; set; }
    }
}
