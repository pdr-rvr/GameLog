using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Services
{
    public class EmpresaServices
    {
        private readonly GameLogContext _context;

        public EmpresaServices(GameLogContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmpresaDTO>> ListarEmpresas()
        {
            var empresas = await _context.Empresa
                .AsNoTracking()
                .Where(e => e.EstaAtivo)
                .OrderBy(e => e.NomeEmpresa)
                .ToListAsync();

            if (!empresas.Any())
                return Enumerable.Empty<EmpresaDTO>();

            // Jogos agrupados considerando desenvolvedora OU publicadora
            var todosJogos = await _context.Jogos
                .AsNoTracking()
                .Where(j => j.EstaAtivo)
                .Select(j => new
                {
                    JogoId = j.Id,
                    DevId = j.Empresa != null ? j.Empresa.Id : (Guid?)null,
                    PubId = j.Publicadora != null ? j.Publicadora.Id : (Guid?)null
                })
                .ToListAsync();

            var jogosStats = new Dictionary<Guid, int>();
            foreach (var j in todosJogos)
            {
                if (j.DevId.HasValue)
                {
                    jogosStats[j.DevId.Value] = jogosStats.GetValueOrDefault(j.DevId.Value, 0) + 1;
                }
                if (j.PubId.HasValue && j.PubId.Value != j.DevId)
                {
                    jogosStats[j.PubId.Value] = jogosStats.GetValueOrDefault(j.PubId.Value, 0) + 1;
                }
            }

            // Média de notas
            var notasPorJogo = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.EstaAtivo && a.Jogo != null)
                .GroupBy(a => a.Jogo.Id)
                .Select(g => new
                {
                    JogoId = g.Key,
                    Media = g.Average(x => (double)x.Nota)
                })
                .ToDictionaryAsync(x => x.JogoId, x => x.Media);

            return empresas.Select(e =>
            {
                jogosStats.TryGetValue(e.Id, out var total);
                
                var jogosDaEmpresa = todosJogos
                    .Where(j => j.DevId == e.Id || j.PubId == e.Id)
                    .Select(j => j.JogoId)
                    .ToList();

                double? media = null;
                var notasDaEmpresa = jogosDaEmpresa
                    .Where(id => notasPorJogo.ContainsKey(id))
                    .Select(id => notasPorJogo[id])
                    .ToList();

                if (notasDaEmpresa.Any())
                {
                    media = notasDaEmpresa.Average();
                }

                return new EmpresaDTO
                {
                    EmpresaId = e.Id,
                    NomeEmpresa = e.NomeEmpresa,
                    EstaAtivo = e.EstaAtivo,
                    TotalJogos = total,
                    MediaNotasJogos = media
                };
            }).ToList();
        }

        public async Task<EmpresaDTO?> ObterEmpresaPorId(Guid id)
        {
            var empresa = await _context.Empresa
                .AsNoTracking()
                .Where(e => e.Id == id && e.EstaAtivo)
                .FirstOrDefaultAsync();

            if (empresa == null) return null;

            var totalJogos = await _context.Jogos
                .AsNoTracking()
                .CountAsync(j => j.EstaAtivo && ((j.Empresa != null && j.Empresa.Id == id) || (j.Publicadora != null && j.Publicadora.Id == id)));

            var mediaNotas = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.EstaAtivo && a.Jogo != null && ((a.Jogo.Empresa != null && a.Jogo.Empresa.Id == id) || (a.Jogo.Publicadora != null && a.Jogo.Publicadora.Id == id)))
                .AverageAsync(a => (double?)a.Nota);

            return new EmpresaDTO
            {
                EmpresaId = empresa.Id,
                NomeEmpresa = empresa.NomeEmpresa,
                EstaAtivo = empresa.EstaAtivo,
                TotalJogos = totalJogos,
                MediaNotasJogos = mediaNotas
            };
        }

        public async Task<IEnumerable<JogoDTO>> ListarJogosPorEmpresa(Guid empresaId)
        {
            var jogos = await _context.Jogos
                .AsNoTracking()
                .Where(j => j.EstaAtivo && ((j.Empresa != null && j.Empresa.Id == empresaId) || (j.Publicadora != null && j.Publicadora.Id == empresaId)))
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Include(j => j.Publicadora)
                .ToListAsync();

            if (!jogos.Any())
                return Enumerable.Empty<JogoDTO>();

            var jogoIds = jogos.Select(j => j.Id).ToList();

            var stats = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.EstaAtivo && a.Jogo != null && jogoIds.Contains(a.Jogo.Id))
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
                    EmpresaId = j.Empresa?.Id ?? Guid.Empty,
                    NomeEmpresa = j.Empresa?.NomeEmpresa ?? string.Empty,
                    PublicadoraId = j.Publicadora?.Id,
                    NomePublicadora = j.Publicadora?.NomeEmpresa,
                    EstaAtivo = j.EstaAtivo,
                    Generos = j.Generos.Select(g => g.TituloGenero).ToList(),
                    MediaAvaliacoes = s?.Media,
                    TotalAvaliacoes = s?.Total ?? 0
                };
            })
            .OrderByDescending(j => j.MediaAvaliacoes ?? 0)
            .ToList();
        }
    }
}
