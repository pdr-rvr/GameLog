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
