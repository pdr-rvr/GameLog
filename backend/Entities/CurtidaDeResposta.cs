using System;

namespace GameLog_Backend.Entities
{
    public class CurtidaDeResposta : Entity<Guid>
    {
        public bool Curtida { get; set; }
        public Guid? RespostaDeAvaliacaoId { get; set; }
        public virtual RespostaDeAvaliacao? RespostaDeAvaliacao { get; set; }
        public Guid? UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }
    }
}
