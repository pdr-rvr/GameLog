using System;
using System.Collections.Generic;

namespace GameLog_Backend.Entities
{
    public class Genero : Entity<Guid>
    {
        public string TituloGenero { get; set; } = string.Empty;
        public virtual ICollection<Jogo> Jogos { get; set; }

        public Genero()
        {
            Jogos = new List<Jogo>();
        }
    }
}
