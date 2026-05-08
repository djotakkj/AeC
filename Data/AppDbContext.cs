using AeC.Enderecos.Models;
using Microsoft.EntityFrameworkCore;

namespace AeC.Enderecos.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Endereco> Enderecos { get; set; }
}