namespace GameLog_Backend.Entities
{
    public class RespostaDeAvaliacao : Entity<int>
    {
        public string Comentario { get; set; } = string.Empty;
        public int? AvaliacaoId { get; set; }
        public virtual Avaliacao? Avaliacao { get; set; }
        public int? UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
