namespace GameLog_Backend.Entities
{
    public class Genero : Entity<int>
    {
        public string TituloGenero { get; set; }
        public virtual ICollection<Jogo> Jogos { get; set; }

        public Genero()
        {
            Jogos = new List<Jogo>();
        }
    }
}
