using Microsoft.EntityFrameworkCore;
using MottuApi.Models;

namespace MottuApi.Data
{
    public class MottuDbContext : DbContext
    {
        public MottuDbContext(DbContextOptions<MottuDbContext> options) : base(options) { }

        public DbSet<Patio> Patios { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Gerente> Gerentes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurações de relacionamento
            modelBuilder.Entity<Patio>()
                .HasOne(p => p.Gerente)
                .WithOne(g => g.Patio)
                .HasForeignKey<Gerente>(g => g.PatioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Funcionario>()
                .HasOne(f => f.Patio)
                .WithMany()
                .HasForeignKey(f => f.PatioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Gerente>()
                .HasOne(g => g.Funcionario)
                .WithOne()
                .HasForeignKey<Gerente>(g => g.FuncionarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}