using System;
using System.Collections.Generic;

namespace GameLog_Backend.Entities
{
    public class RespostaDeAvaliacao : Entity<Guid>
    {
        public string Comentario { get; set; } = string.Empty;
        public Guid? AvaliacaoId { get; set; }
        public virtual Avaliacao? Avaliacao { get; set; }
        public Guid? UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public virtual ICollection<CurtidaDeResposta> CurtidasDeRespostas { get; set; }

        public RespostaDeAvaliacao()
        {
            CurtidasDeRespostas = new List<CurtidaDeResposta>();
        }
    }
}
