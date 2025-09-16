using Microsoft.EntityFrameworkCore;
using MottuApi.Models;
using MottuApi.Services;

namespace MottuApi.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(MottuDbContext context)
        {
            // Verificar se já existem dados
            if (await context.Patios.AnyAsync())
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
                }
            };

            // Hash das senhas
            var authService = new AuthService(context);
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
