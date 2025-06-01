using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GameLog_Backend.Services
{
    public class JogoServices
    {
        protected readonly GameLogContext context;

        public JogoServices(GameLogContext context)
        {
            this.context = context;
        }

        public IEnumerable<JogoDTO> ListarJogos()
        {
            var jogos = context.Jogos
                .Include(j => j.Generos) 
                .Include(j => j.Empresa)  
                .Select(g => new JogoDTO
                {
                    JogoId = g.Id,
                    Titulo = g.Titulo,
                    Descricao = g.Descricao,
                    Imagem = g.Imagem,
                    DataLancamento = g.DataLancamento,
                    ClassificacaoIndicativa = g.ClassificacaoIndicativa,
                    EmpresaId = g.Empresa.Id,
                    EstaAtivo = g.EstaAtivo
                }).ToList();

            return jogos;
        }

        public IEnumerable<JogoDTO> ListarTop3JogosMelhorAvaliados()
        {
            var topJogos = context.Avaliacoes
                .Where(a => a.EstaAtivo && a.Jogo.EstaAtivo)
                .GroupBy(a => a.Jogo)
                .Select(g => new JogoDTO
                {
                    JogoId = g.Key.Id,
                    Titulo = g.Key.Titulo,
                    Descricao = g.Key.Descricao,
                    Imagem = g.Key.Imagem,
                    DataLancamento = g.Key.DataLancamento,
                    ClassificacaoIndicativa = g.Key.ClassificacaoIndicativa,
                    EmpresaId = g.Key.Empresa.Id,
                    EstaAtivo = g.Key.EstaAtivo,
                    MediaAvaliacoes = g.Average(a => a.Nota)
                })
                .OrderByDescending(j => j.MediaAvaliacoes)
                .ThenByDescending(j => j.DataLancamento) 
                .Take(3)
                .ToList();

            return topJogos;
        }
    }
}