using GameLog_Backend.Configurations;
using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Database
{
    public class GameLogContext : DbContext
    {
        public GameLogContext(DbContextOptions<GameLogContext> options) : base(options) { }
        
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Genero> Generos { get; set; }
        public DbSet<Empresa> Empresa { get; set; }
        public DbSet<Jogo> Jogos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<RespostaDeAvaliacao> RespostasDeAvaliacao { get; set; }
        public DbSet<CurtidaDeAvaliacao> CurtidasDeAvaliacoes { get; set; }
        public DbSet<SegueUsuario> SegueUsuarios { get; set;}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine);
            //optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UsuarioConfigurations());
            modelBuilder.ApplyConfiguration(new GeneroConfigurations());
            modelBuilder.ApplyConfiguration(new EmpresaConfigurations());
            modelBuilder.ApplyConfiguration(new JogoConfigurations());
            modelBuilder.ApplyConfiguration(new AvaliacaoConfigurations());
            modelBuilder.ApplyConfiguration(new RespostaDeAvaliacaoConfigurations());
            modelBuilder.ApplyConfiguration(new CurtidaDeAvaliacaoConfigurations());
            modelBuilder.ApplyConfiguration(new SegueUsuarioConfigurations());
        }
    }

    
}
