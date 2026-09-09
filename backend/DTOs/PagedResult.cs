using System;
using System.Collections.Generic;

namespace GameLog_Backend.DTOs
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Itens { get; set; } = new List<T>();
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalItens { get; set; }
        public int ItensPorPagina { get; set; }
        public bool TemAnterior => PaginaAtual > 1;
        public bool TemProxima => PaginaAtual < TotalPaginas;

        public PagedResult() { }

        public PagedResult(IEnumerable<T> itens, int totalItens, int paginaAtual, int itensPorPagina)
        {
            Itens = itens;
            TotalItens = totalItens;
            PaginaAtual = paginaAtual;
            ItensPorPagina = itensPorPagina;
            TotalPaginas = (int)Math.Ceiling(totalItens / (double)itensPorPagina);
        }
    }
}
