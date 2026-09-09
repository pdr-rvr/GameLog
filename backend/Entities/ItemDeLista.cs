using System;

namespace GameLog_Backend.Entities
{
    public class ItemDeLista : Entity<int>
    {
        public int ListaDeJogosId { get; set; }
        public virtual ListaDeJogos ListaDeJogos { get; set; }

        public int JogoId { get; set; }
        public virtual Jogo Jogo { get; set; }

        public int Ordem { get; set; }
        public DateTime DataAdicionado { get; set; } = DateTime.UtcNow;
    }
}
