using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Services
{
    public class EmpresaServices : IEmpresaService
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

            // 1. Contagens agregadas diretamente no PostgreSQL
            var statsDev = await _context.Jogos
                .AsNoTracking()
                .Where(j => j.EstaAtivo && j.Empresa != null)
                .GroupBy(j => j.Empresa!.Id)
                .Select(g => new { EmpresaId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.EmpresaId, x => x.Total);

            var statsPub = await _context.Jogos
                .AsNoTracking()
                .Where(j => j.EstaAtivo && j.Publicadora != null && j.Publicadora.Id != j.Empresa.Id)
                .GroupBy(j => j.Publicadora!.Id)
                .Select(g => new { EmpresaId = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.EmpresaId, x => x.Total);

            // 2. Média de notas agregada no banco (desenvolvedoras e publicadoras)
            var mediasDev = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.EstaAtivo && a.Jogo != null && a.Jogo.EstaAtivo && a.Jogo.Empresa != null)
                .GroupBy(a => a.Jogo.Empresa!.Id)
                .Select(g => new
                {
                    EmpresaId = g.Key,
                    Media = g.Average(x => (double)x.Nota)
                })
                .ToDictionaryAsync(x => x.EmpresaId, x => x.Media);

            var mediasPub = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.EstaAtivo && a.Jogo != null && a.Jogo.EstaAtivo && a.Jogo.Publicadora != null)
                .GroupBy(a => a.Jogo.Publicadora!.Id)
                .Select(g => new
                {
                    EmpresaId = g.Key,
                    Media = g.Average(x => (double)x.Nota)
                })
                .ToDictionaryAsync(x => x.EmpresaId, x => x.Media);

            return empresas.Select(e =>
            {
                var total = statsDev.GetValueOrDefault(e.Id, 0) + statsPub.GetValueOrDefault(e.Id, 0);
                var media = mediasDev.ContainsKey(e.Id) 
                    ? mediasDev[e.Id] 
                    : (mediasPub.ContainsKey(e.Id) ? mediasPub[e.Id] : 0.0);

                return new EmpresaDTO
                {
                    EmpresaId = e.Id,
                    NomeEmpresa = e.NomeEmpresa,
                    EstaAtivo = e.EstaAtivo,
                    TotalJogos = total,
                    MediaNotasJogos = media > 0 ? Math.Round(media, 1) : null
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
