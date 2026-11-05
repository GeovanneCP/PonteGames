using Microsoft.EntityFrameworkCore;
using PonteGames.Api.Models;

namespace PonteGames.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Descarte> Descartes => Set<Descarte>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Garante que a chave de idempotência é única no banco de dados
        modelBuilder.Entity<Descarte>()
            .HasIndex(d => d.IdempotencyKey)
            .IsUnique();
    }
}