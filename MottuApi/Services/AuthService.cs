using Microsoft.EntityFrameworkCore;
using MottuApi.Data;
using MottuApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace MottuApi.Services
{
    public class AuthService
    {
        private readonly MottuDbContext _context;

        public AuthService(MottuDbContext context)
        {
            _context = context;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var funcionario = await _context.Funcionarios
                .Include(f => f.Patio)
                .FirstOrDefaultAsync(f => f.Email == request.Email);

            if (funcionario == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Email ou senha inválidos"
                };
            }

            if (!VerifyPassword(request.Senha, funcionario.Senha))
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Email ou senha inválidos"
                };
            }

            // Gerar token simples (em produção, use JWT)
            var token = GenerateSimpleToken(funcionario);

            return new LoginResponse
            {
                Success = true,
                Message = "Login realizado com sucesso",
                Token = token,
                Funcionario = funcionario
            };
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        private string GenerateSimpleToken(Funcionario funcionario)
        {
            var tokenData = $"{funcionario.Id}:{funcionario.Email}:{DateTime.UtcNow:O}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenData));
        }
    }
}
