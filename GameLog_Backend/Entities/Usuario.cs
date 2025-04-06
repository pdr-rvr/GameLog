namespace GameLog_Backend.Entities
{
    public class Usuario : Entity<int>
    {
        public string NomeUsuario { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string FotoDePerfil { get; set; }
        public virtual ICollection<Avaliacao> Avaliacoes { get; set; }

        public Usuario()
        {
            Avaliacoes = new List<Avaliacao>();
        }
    }
}
