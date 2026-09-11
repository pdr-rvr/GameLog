using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Configurations
{
    public class CurtidaDeAvaliacaoConfigurations : IEntityTypeConfiguration<CurtidaDeAvaliacao>
    {
        public void Configure(EntityTypeBuilder<CurtidaDeAvaliacao> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName($"{nameof(CurtidaDeAvaliacao)}Id")
                .IsRequired();

            builder.Property(p => p.Curtida)
                .IsRequired();

            builder.Property(p => p.EstaAtivo)
                .IsRequired();

            builder.HasOne(c => c.Avaliacao)
                .WithMany(a => a.CurtidasDeAvaliacao)
                .HasForeignKey(c => c.AvaliacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
