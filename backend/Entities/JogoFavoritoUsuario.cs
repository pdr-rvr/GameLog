using System;

namespace GameLog_Backend.Entities
{
    public class JogoFavoritoUsuario : Entity<Guid>
    {
        public Guid UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; } = null!;

        public Guid JogoId { get; set; }
        public virtual Jogo Jogo { get; set; } = null!;

        // Posição de 1 a 5 no pódio
        public int Posicao { get; set; }
    }
}
