using System;

namespace GameLog_Backend.Entities
{
    public class CurtidaDeAvaliacao : Entity<Guid>
    {
        public bool Curtida { get; set; }
        public Guid? AvaliacaoId { get; set; }
        public virtual Avaliacao? Avaliacao { get; set; }
        public Guid? UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }
    }
}
