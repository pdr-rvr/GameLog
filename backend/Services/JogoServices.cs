using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Services
{
    public class JogoServices
    {
        protected readonly GameLogContext _context;

        public JogoServices(GameLogContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JogoDTO>> ListarJogos()
        {
            return await _context.Jogos
                .Where(j => j.EstaAtivo)
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Select(j => new JogoDTO
                {
                    JogoId = j.Id,
                    Titulo = j.Titulo,
                    Descricao = j.Descricao,
                    Imagem = j.Imagem,
                    DataLancamento = j.DataLancamento,
                    ClassificacaoIndicativa = j.ClassificacaoIndicativa,
                    EmpresaId = j.Empresa.Id,
                    NomeEmpresa = j.Empresa.NomeEmpresa,
                    EstaAtivo = j.EstaAtivo,
                    Generos = j.Generos.Select(g => g.TituloGenero).ToList(),
                    MediaAvaliacoes = _context.Avaliacoes
                        .Where(a => a.Jogo.Id == j.Id && a.EstaAtivo)
                        .Average(a => (double?)a.Nota),
                    TotalAvaliacoes = _context.Avaliacoes
                        .Count(a => a.Jogo.Id == j.Id && a.EstaAtivo)
                })
                .ToListAsync();
        }

        public async Task<PagedResult<JogoDTO>> ListarJogosPaginados(
            int pagina = 1,
            int itensPorPagina = 12,
            string? busca = null,
            string? genero = null,
            int? ano = null,
            string? empresa = null,
            string ordenacao = "melhores")
        {
            pagina = Math.Max(1, pagina);
            itensPorPagina = Math.Clamp(itensPorPagina, 1, 100);

            var query = _context.Jogos
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Where(j => j.EstaAtivo);

            // Filtros
            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                query = query.Where(j => j.Titulo.ToLower().Contains(termo)
                                      || (j.Descricao != null && j.Descricao.ToLower().Contains(termo))
                                      || (j.Empresa != null && j.Empresa.NomeEmpresa.ToLower().Contains(termo)));
            }

            if (!string.IsNullOrWhiteSpace(genero))
            {
                query = query.Where(j => j.Generos.Any(g => g.TituloGenero.ToLower() == genero.Trim().ToLower()));
            }

            if (ano.HasValue)
            {
                query = query.Where(j => j.DataLancamento.Year == ano.Value);
            }

            if (!string.IsNullOrWhiteSpace(empresa))
            {
                query = query.Where(j => j.Empresa != null && j.Empresa.NomeEmpresa.ToLower() == empresa.Trim().ToLower());
            }

            var totalItens = await query.CountAsync();

            // Consulta com projeção
            var jogosQuery = query.Select(j => new JogoDTO
            {
                JogoId = j.Id,
                Titulo = j.Titulo,
                Descricao = j.Descricao,
                Imagem = j.Imagem,
                DataLancamento = j.DataLancamento,
                ClassificacaoIndicativa = j.ClassificacaoIndicativa,
                EmpresaId = j.Empresa.Id,
                NomeEmpresa = j.Empresa.NomeEmpresa,
                EstaAtivo = j.EstaAtivo,
                Generos = j.Generos.Select(g => g.TituloGenero).ToList(),
                MediaAvaliacoes = _context.Avaliacoes
                    .Where(a => a.Jogo.Id == j.Id && a.EstaAtivo)
                    .Average(a => (double?)a.Nota),
                TotalAvaliacoes = _context.Avaliacoes
                    .Count(a => a.Jogo.Id == j.Id && a.EstaAtivo)
            });

            // Ordenação
            jogosQuery = ordenacao.ToLower() switch
            {
                "recentes" => jogosQuery.OrderByDescending(j => j.DataLancamento),
                "antigos" => jogosQuery.OrderBy(j => j.DataLancamento),
                "az" => jogosQuery.OrderBy(j => j.Titulo),
                "za" => jogosQuery.OrderByDescending(j => j.Titulo),
                _ => jogosQuery.OrderByDescending(j => j.MediaAvaliacoes ?? 0).ThenByDescending(j => j.DataLancamento)
            };

            var itens = await jogosQuery
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToListAsync();

            return new PagedResult<JogoDTO>(itens, totalItens, pagina, itensPorPagina);
        }

        public async Task<JogoDTO?> ObterJogoPorId(int id)
        {
            return await _context.Jogos
                .Where(j => j.Id == id && j.EstaAtivo)
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Select(j => new JogoDTO
                {
                    JogoId = j.Id,
                    Titulo = j.Titulo,
                    Descricao = j.Descricao,
                    Imagem = j.Imagem,
                    DataLancamento = j.DataLancamento,
                    ClassificacaoIndicativa = j.ClassificacaoIndicativa,
                    EmpresaId = j.Empresa.Id,
                    NomeEmpresa = j.Empresa.NomeEmpresa,
                    EstaAtivo = j.EstaAtivo,
                    Generos = j.Generos.Select(g => g.TituloGenero).ToList(),
                    MediaAvaliacoes = _context.Avaliacoes
                        .Where(a => a.Jogo.Id == j.Id && a.EstaAtivo)
                        .Average(a => (double?)a.Nota),
                    TotalAvaliacoes = _context.Avaliacoes
                        .Count(a => a.Jogo.Id == j.Id && a.EstaAtivo)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<JogoDTO>> ListarTop10JogosMelhorAvaliados()
        {
            return await _context.Avaliacoes
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
                    NomeEmpresa = g.Key.Empresa.NomeEmpresa,
                    EstaAtivo = g.Key.EstaAtivo,
                    MediaAvaliacoes = g.Average(a => (double?)a.Nota),
                    TotalAvaliacoes = g.Count(),
                    Generos = g.Key.Generos.Select(ge => ge.TituloGenero).ToList()
                })
                .OrderByDescending(j => j.MediaAvaliacoes)
                .ThenByDescending(j => j.DataLancamento)
                .Take(10)
                .ToListAsync();
        }
    }
}
