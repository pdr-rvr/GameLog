namespace GameLog_Backend.Entities
{
    public class Jogo : Entity<int>
    {
        public string Titulo {  get; set; }
        public string Descricao { get; set; }
        public string Imagem {  get; set; }
        public DateOnly DataLancamento { get; set; }
        public virtual ICollection<Genero> Generos { get; set; }
        public int ClassificacaoIndicativa { get; set; }
        public virtual Empresa Empresa { get; set; }

        public Jogo()
        {
            Generos = new List<Genero>();
        }
    }
}
