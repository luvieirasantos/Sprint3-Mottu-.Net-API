using Microsoft.EntityFrameworkCore;
using MottuApi.Data;
using MottuApi.Models;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace MottuApi.Services
{
    public class AuthService
    {
        private readonly MottuDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(MottuDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

            // Gerar token JWT
            var token = GenerateJwtToken(funcionario);

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

        private string GenerateJwtToken(Funcionario funcionario)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "MottuApiSecretKeyForDevelopmentAndProduction2025!@#$%";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "MottuApi";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "MottuApiUsers";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, funcionario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, funcionario.Id.ToString()),
                new Claim(ClaimTypes.Name, funcionario.Nome),
                new Claim(ClaimTypes.Email, funcionario.Email)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
