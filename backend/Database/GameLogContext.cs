using System;
using System.Threading;
using System.Threading.Tasks;
using GameLog_Backend.Configurations;
using GameLog_Backend.Entities;
using GameLog_Backend.Helpers;
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
        public DbSet<CurtidaDeResposta> CurtidasDeRespostas { get; set; }
        public DbSet<SegueUsuario> SegueUsuarios { get; set; }
        public DbSet<BibliotecaJogo> ItensBiblioteca { get; set; }
        public DbSet<JogoFavoritoUsuario> JogosFavoritosUsuarios { get; set; }
        public DbSet<ListaDeJogos> ListasDeJogos { get; set; }
        public DbSet<ItemDeLista> ItensDeListas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public override int SaveChanges()
        {
            AssignUuidV7Ids();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AssignUuidV7Ids();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void AssignUuidV7Ids()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity is Entity<Guid> entityWithGuid && entityWithGuid.Id == Guid.Empty)
                    {
                        entityWithGuid.Id = UuidV7Helper.NewGuid();
                    }
                }
            }
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
            modelBuilder.ApplyConfiguration(new CurtidaDeRespostaConfigurations());
            modelBuilder.ApplyConfiguration(new SegueUsuarioConfigurations());
            modelBuilder.ApplyConfiguration(new BibliotecaConfigurations());
            modelBuilder.ApplyConfiguration(new JogoFavoritoConfigurations());
            modelBuilder.ApplyConfiguration(new ListaDeJogosConfigurations());
            modelBuilder.ApplyConfiguration(new ItemDeListaConfigurations());
        }
    }
}
