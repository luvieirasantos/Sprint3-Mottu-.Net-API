namespace MottuApi.Models
{
    public class FuncionarioResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int PatioId { get; set; }
        public Patio? Patio { get; set; }
    }
}
