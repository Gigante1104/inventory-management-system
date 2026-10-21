using InventarioApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioApi.Data;

// El esquema lo crean los scripts de /database, por eso no usamos migraciones.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // La fecha de creación la pone PostgreSQL (DEFAULT NOW()), EF no debe enviarla
        modelBuilder.Entity<Producto>().Property(p => p.CreadoEn).HasDefaultValueSql("NOW()");
        modelBuilder.Entity<Usuario>().Property(u => u.CreadoEn).HasDefaultValueSql("NOW()");
    }
}
