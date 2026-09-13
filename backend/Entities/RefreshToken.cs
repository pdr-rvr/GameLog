using System;

namespace GameLog_Backend.Entities
{
    public class RefreshToken : Entity<Guid>
    {
        public string Token { get; set; } = string.Empty;
        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataExpiracao { get; set; }
        public DateTime? RevogadoEm { get; set; }
        public string? CriadoPorIp { get; set; }
        public string? RevogadoPorIp { get; set; }
        public string? SubstituidoPorToken { get; set; }

        public bool EstaExpirado => DateTime.UtcNow >= DataExpiracao;
        public bool EstaRevogado => RevogadoEm != null;
        public bool EstaValido => !EstaRevogado && !EstaExpirado && EstaAtivo;
    }
}
