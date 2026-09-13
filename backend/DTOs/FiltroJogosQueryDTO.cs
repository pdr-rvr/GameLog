using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace GameLog_Backend.DTOs
{
    public class FiltroJogosQueryDTO
    {
        [FromQuery]
        public int? Pagina { get; set; }

        [FromQuery]
        public int? ItensPorPagina { get; set; }

        [FromQuery]
        public string? Busca { get; set; }

        [FromQuery(Name = "genero")]
        public string[]? Genero { get; set; }

        [FromQuery(Name = "generos")]
        public string? GenerosComma { get; set; }

        [FromQuery]
        public int? Ano { get; set; }

        [FromQuery]
        public string? Empresa { get; set; }

        [FromQuery]
        public double? NotaMinima { get; set; }

        [FromQuery]
        public string? Ordenacao { get; set; }

        public List<string>? ObterListaGenerosNormalizada()
        {
            var listaGeneros = new List<string>();
            if (Genero != null && Genero.Length > 0)
            {
                foreach (var g in Genero)
                {
                    if (!string.IsNullOrWhiteSpace(g))
                    {
                        var parts = g.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        listaGeneros.AddRange(parts);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(GenerosComma))
            {
                var parts = GenerosComma.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                listaGeneros.AddRange(parts);
            }

            var distinct = listaGeneros.Distinct().ToList();
            return distinct.Any() ? distinct : null;
        }
    }
}
