using System;

namespace GameLog_Backend.Entities
{
    public class ItemDeLista : Entity<Guid>
    {
        public Guid ListaDeJogosId { get; set; }
        public virtual ListaDeJogos ListaDeJogos { get; set; } = null!;

        public Guid JogoId { get; set; }
        public virtual Jogo Jogo { get; set; } = null!;

        public int Ordem { get; set; }
        public DateTime DataAdicionado { get; set; } = DateTime.UtcNow;
    }
}
