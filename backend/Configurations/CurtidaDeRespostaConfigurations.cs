using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Configurations
{
    public class CurtidaDeRespostaConfigurations : IEntityTypeConfiguration<CurtidaDeResposta>
    {
        public void Configure(EntityTypeBuilder<CurtidaDeResposta> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(e => e.Id)
                .HasColumnName($"{nameof(CurtidaDeResposta)}Id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(p => p.Curtida)
                .IsRequired();

            builder.Property(p => p.EstaAtivo)
                .IsRequired();

            builder.HasOne(c => c.RespostaDeAvaliacao)
                .WithMany(r => r.CurtidasDeRespostas)
                .HasForeignKey(c => c.RespostaDeAvaliacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
