namespace GameLog_Backend.Entities
{
    public class CurtidaDeAvaliacao : Entity<int>
    {
        public bool Curtida { get; set; }
        public int? AvaliacaoId { get; set; }
        public virtual Avaliacao? Avaliacao { get; set; }
        public int? UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }
    }
}
