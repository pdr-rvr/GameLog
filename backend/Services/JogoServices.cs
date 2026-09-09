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
            var jogos = await _context.Jogos
                .AsNoTracking()
                .Where(j => j.EstaAtivo)
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .ToListAsync();

            if (!jogos.Any())
                return Enumerable.Empty<JogoDTO>();

            // Otimização em lote: 1 query de agregação para todas as avaliações ativas
            var stats = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.EstaAtivo)
                .GroupBy(a => a.Jogo.Id)
                .Select(g => new
                {
                    JogoId = g.Key,
                    Media = g.Average(x => (double)x.Nota),
                    Total = g.Count()
                })
                .ToDictionaryAsync(x => x.JogoId);

            return jogos.Select(j =>
            {
                stats.TryGetValue(j.Id, out var s);
                return new JogoDTO
                {
                    JogoId = j.Id,
                    Titulo = j.Titulo,
                    Descricao = j.Descricao,
                    Imagem = j.Imagem,
                    DataLancamento = j.DataLancamento,
                    ClassificacaoIndicativa = j.ClassificacaoIndicativa,
                    EmpresaId = j.Empresa?.Id ?? 0,
                    NomeEmpresa = j.Empresa?.NomeEmpresa ?? string.Empty,
                    EstaAtivo = j.EstaAtivo,
                    Generos = j.Generos.Select(g => g.TituloGenero).ToList(),
                    MediaAvaliacoes = s != null ? s.Media : null,
                    TotalAvaliacoes = s?.Total ?? 0
                };
            }).ToList();
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
                .AsNoTracking()
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Where(j => j.EstaAtivo);

            // Filtros com sanitização
            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                query = query.Where(j => j.Titulo.ToLower().Contains(termo)
                                      || (j.Descricao != null && j.Descricao.ToLower().Contains(termo))
                                      || (j.Empresa != null && j.Empresa.NomeEmpresa.ToLower().Contains(termo)));
            }

            if (!string.IsNullOrWhiteSpace(genero))
            {
                var generoTerm = genero.Trim().ToLower();
                query = query.Where(j => j.Generos.Any(g => g.TituloGenero.ToLower() == generoTerm));
            }

            if (ano.HasValue)
            {
                query = query.Where(j => j.DataLancamento.Year == ano.Value);
            }

            if (!string.IsNullOrWhiteSpace(empresa))
            {
                var empresaTerm = empresa.Trim().ToLower();
                query = query.Where(j => j.Empresa != null && j.Empresa.NomeEmpresa.ToLower() == empresaTerm);
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
            jogosQuery = (ordenacao ?? "melhores").ToLower() switch
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
            var jogo = await _context.Jogos
                .AsNoTracking()
                .Where(j => j.Id == id && j.EstaAtivo)
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .FirstOrDefaultAsync();

            if (jogo == null) return null;

            var stats = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Jogo.Id == id && a.EstaAtivo)
                .GroupBy(a => a.Jogo.Id)
                .Select(g => new
                {
                    Media = g.Average(x => (double)x.Nota),
                    Total = g.Count()
                })
                .FirstOrDefaultAsync();

            return new JogoDTO
            {
                JogoId = jogo.Id,
                Titulo = jogo.Titulo,
                Descricao = jogo.Descricao,
                Imagem = jogo.Imagem,
                DataLancamento = jogo.DataLancamento,
                ClassificacaoIndicativa = jogo.ClassificacaoIndicativa,
                EmpresaId = jogo.Empresa?.Id ?? 0,
                NomeEmpresa = jogo.Empresa?.NomeEmpresa ?? string.Empty,
                EstaAtivo = jogo.EstaAtivo,
                Generos = jogo.Generos.Select(g => g.TituloGenero).ToList(),
                MediaAvaliacoes = stats?.Media,
                TotalAvaliacoes = stats?.Total ?? 0
            };
        }

        public async Task<IEnumerable<JogoDTO>> ListarTop10JogosMelhorAvaliados()
        {
            return await _context.Avaliacoes
                .AsNoTracking()
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
