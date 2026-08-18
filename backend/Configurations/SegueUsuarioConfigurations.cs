using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Configurations
{
    public class SegueUsuarioConfigurations : IEntityTypeConfiguration<SegueUsuario>
    {
        public void Configure(EntityTypeBuilder<SegueUsuario> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(e => e.UsuarioSeguido)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.UsuarioSeguidor)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.EstaAtivo)
                .IsRequired();
        }
    }
}
