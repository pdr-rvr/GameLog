using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameLog_Backend.Configurations
{
    public class ListaDeJogosConfigurations : IEntityTypeConfiguration<ListaDeJogos>
    {
        public void Configure(EntityTypeBuilder<ListaDeJogos> builder)
        {
            builder.ToTable("ListasDeJogos");
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Id).HasColumnName("ListaDeJogosId");

            builder.Property(l => l.Titulo)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(l => l.Descricao)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(l => l.EstaPublica)
                .HasDefaultValue(true);

            builder.Property(l => l.EstaAtivo)
                .HasDefaultValue(true);

            builder.HasOne(l => l.Usuario)
                .WithMany()
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.Itens)
                .WithOne(i => i.ListaDeJogos)
                .HasForeignKey(i => i.ListaDeJogosId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(l => l.UsuarioId);
        }
    }

    public class ItemDeListaConfigurations : IEntityTypeConfiguration<ItemDeLista>
    {
        public void Configure(EntityTypeBuilder<ItemDeLista> builder)
        {
            builder.ToTable("ItensDeListas");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).HasColumnName("ItemDeListaId");

            builder.Property(i => i.EstaAtivo)
                .HasDefaultValue(true);

            builder.HasOne(i => i.ListaDeJogos)
                .WithMany(l => l.Itens)
                .HasForeignKey(i => i.ListaDeJogosId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.Jogo)
                .WithMany()
                .HasForeignKey(i => i.JogoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(i => new { i.ListaDeJogosId, i.JogoId }).IsUnique();
        }
    }
}
