namespace MottuApi.Models
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public Funcionario? Funcionario { get; set; }
    }
}
