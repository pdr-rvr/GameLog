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

            // Contagem de jogos agrupada por empresa
            var jogosStats = await _context.Jogos
                .AsNoTracking()
                .Where(j => j.EstaAtivo && j.Empresa != null)
                .GroupBy(j => j.Empresa.Id)
                .Select(g => new
                {
                    EmpresaId = g.Key,
                    TotalJogos = g.Count()
                })
                .ToDictionaryAsync(x => x.EmpresaId, x => x.TotalJogos);

            // Média de notas de jogos agrupada por empresa
            var notasStats = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.EstaAtivo && a.Jogo != null && a.Jogo.Empresa != null)
                .GroupBy(a => a.Jogo.Empresa.Id)
                .Select(g => new
                {
                    EmpresaId = g.Key,
                    Media = g.Average(x => (double)x.Nota)
                })
                .ToDictionaryAsync(x => x.EmpresaId, x => x.Media);

            return empresas.Select(e => new EmpresaDTO
            {
                EmpresaId = e.Id,
                NomeEmpresa = e.NomeEmpresa,
                EstaAtivo = e.EstaAtivo,
                TotalJogos = jogosStats.TryGetValue(e.Id, out var total) ? total : 0,
                MediaNotasJogos = notasStats.TryGetValue(e.Id, out var media) ? media : null
            }).ToList();
        }

        public async Task<EmpresaDTO?> ObterEmpresaPorId(int id)
        {
            var empresa = await _context.Empresa
                .AsNoTracking()
                .Where(e => e.Id == id && e.EstaAtivo)
                .FirstOrDefaultAsync();

            if (empresa == null) return null;

            var totalJogos = await _context.Jogos
                .AsNoTracking()
                .CountAsync(j => j.Empresa != null && j.Empresa.Id == id && j.EstaAtivo);

            var mediaNotas = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.EstaAtivo && a.Jogo != null && a.Jogo.Empresa != null && a.Jogo.Empresa.Id == id)
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

        public async Task<IEnumerable<JogoDTO>> ListarJogosPorEmpresa(int empresaId)
        {
            var jogos = await _context.Jogos
                .AsNoTracking()
                .Where(j => j.Empresa != null && j.Empresa.Id == empresaId && j.EstaAtivo)
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
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
                    EmpresaId = j.Empresa?.Id ?? 0,
                    NomeEmpresa = j.Empresa?.NomeEmpresa ?? string.Empty,
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
