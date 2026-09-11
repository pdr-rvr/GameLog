using System;

namespace GameLog_Backend.Entities
{
    public enum StatusJogo
    {
        QueroJogar = 1,
        Jogando = 2,
        Zerado = 3,
        Pausado = 4,
        Abandonado = 5
    }

    public class BibliotecaJogo : Entity<Guid>
    {
        public Guid UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; } = null!;

        public Guid JogoId { get; set; }
        public virtual Jogo Jogo { get; set; } = null!;

        public StatusJogo Status { get; set; }

        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataConclusao { get; set; }
    }
}
