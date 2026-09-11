using System;
using System.Collections.Generic;

namespace GameLog_Backend.Entities
{
    public class Avaliacao : Entity<Guid>
    {
        public int Nota { get; set; }
        public virtual Jogo Jogo { get; set; } = null!;
        public virtual Usuario Usuario { get; set; } = null!;
        public string TextoAvaliacao { get; set; } = string.Empty;
        public DateTime DataPublicacao { get; set; } = DateTime.UtcNow;
        public virtual ICollection<CurtidaDeAvaliacao> CurtidasDeAvaliacao { get; set; }
        public virtual ICollection<RespostaDeAvaliacao> RespostasDeAvaliacao { get; set; }

        public Avaliacao()
        {
            CurtidasDeAvaliacao = new List<CurtidaDeAvaliacao>();
            RespostasDeAvaliacao = new List<RespostaDeAvaliacao>();
        }
    }
}
