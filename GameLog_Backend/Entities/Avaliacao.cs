namespace GameLog_Backend.Entities
{
    public class Avaliacao : Entity<int>
    {
        public int Nota {  get; set; }
        public Jogo Jogo { get; set; }
        public string TextoAvaliacao { get; set; }
        public virtual ICollection<CurtidaDeAvaliacao> CurtidasDeAvaliacao { get; set; }
        public virtual ICollection<RespostaDeAvaliacao> RespostasDeAvaliacao { get; set; }        

        public Avaliacao()
        {
            CurtidasDeAvaliacao = new List<CurtidaDeAvaliacao>();
            RespostasDeAvaliacao = new List<RespostaDeAvaliacao>();
        }
    }
}
