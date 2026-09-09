namespace GameLog_Backend.Entities
{
    public class JogoFavoritoUsuario : Entity<int>
    {
        public int UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; }

        public int JogoId { get; set; }
        public virtual Jogo Jogo { get; set; }

        // Posição de 1 a 5 no pódio
        public int Posicao { get; set; }
    }
}
