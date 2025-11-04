using Microsoft.EntityFrameworkCore;
using MottuApi.Models;
using MottuApi.Services;

namespace MottuApi.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(MottuDbContext context, IConfiguration configuration)
        {
            // Verificar se já existem dados verificando especificamente por um funcionário de teste
            var existingUser = await context.Funcionarios
                .Where(f => f.Email == "joao.silva@mottu.com")
                .FirstOrDefaultAsync();

            if (existingUser != null)
                return;

            // Criar pátios
            var patios = new List<Patio>
            {
                new Patio { Nome = "Pátio Central", Endereco = "Rua das Flores, 123 - Centro" },
                new Patio { Nome = "Pátio Norte", Endereco = "Av. Norte, 456 - Zona Norte" },
                new Patio { Nome = "Pátio Sul", Endereco = "Rua Sul, 789 - Zona Sul" }
            };

            context.Patios.AddRange(patios);
            await context.SaveChangesAsync();

            // Criar funcionários
            var funcionarios = new List<Funcionario>
            {
                new Funcionario
                {
                    Nome = "João Silva",
                    Email = "joao.silva@mottu.com",
                    Senha = "123456", // Será hasheada pelo AuthService
                    PatioId = patios[0].Id
                },
                new Funcionario
                {
                    Nome = "Maria Santos",
                    Email = "maria.santos@mottu.com",
                    Senha = "123456",
                    PatioId = patios[1].Id
                },
                new Funcionario
                {
                    Nome = "Pedro Costa",
                    Email = "pedro.costa@mottu.com",
                    Senha = "123456",
                    PatioId = patios[2].Id
                },
                new Funcionario
                {
                    Nome = "Funcionário 1",
                    Email = "funcionario1@mottu.com",
                    Senha = "senha123",
                    PatioId = patios[0].Id
                }
            };

            // Hash das senhas
            var authService = new AuthService(context, configuration);
            foreach (var funcionario in funcionarios)
            {
                funcionario.Senha = authService.HashPassword(funcionario.Senha);
            }

            context.Funcionarios.AddRange(funcionarios);
            await context.SaveChangesAsync();

            // Criar gerentes
            var gerentes = new List<Gerente>
            {
                new Gerente 
                { 
                    FuncionarioId = funcionarios[0].Id, 
                    PatioId = patios[0].Id 
                },
                new Gerente 
                { 
                    FuncionarioId = funcionarios[1].Id, 
                    PatioId = patios[1].Id 
                }
            };

            context.Gerentes.AddRange(gerentes);
            await context.SaveChangesAsync();
        }
    }
}
