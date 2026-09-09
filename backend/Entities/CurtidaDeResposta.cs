namespace GameLog_Backend.Entities
{
    public class CurtidaDeResposta : Entity<int>
    {
        public bool Curtida { get; set; }
        public int? RespostaDeAvaliacaoId { get; set; }
        public virtual RespostaDeAvaliacao? RespostaDeAvaliacao { get; set; }
        public int? UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }
    }
}
