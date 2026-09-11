using System;
using System.Collections.Generic;

namespace GameLog_Backend.Entities
{
    public class Jogo : Entity<Guid>
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Imagem { get; set; } = string.Empty;
        public DateOnly DataLancamento { get; set; }
        public virtual ICollection<Genero> Generos { get; set; }
        public int ClassificacaoIndicativa { get; set; }
        public virtual Empresa Empresa { get; set; } = null!;
        public virtual Empresa? Publicadora { get; set; }

        public Jogo()
        {
            Generos = new List<Genero>();
        }
    }
}
