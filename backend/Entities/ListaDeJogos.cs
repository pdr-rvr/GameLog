using System;
using System.Collections.Generic;

namespace GameLog_Backend.Entities
{
    public class ListaDeJogos : Entity<Guid>
    {
        public Guid UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; } = null!;

        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public bool EstaPublica { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ItemDeLista> Itens { get; set; } = new List<ItemDeLista>();
    }
}
