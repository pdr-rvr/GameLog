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
            return await _context.Empresa
                .Where(e => e.EstaAtivo)
                .Select(e => new EmpresaDTO
                {
                    EmpresaId = e.Id,
                    NomeEmpresa = e.NomeEmpresa,
                    EstaAtivo = e.EstaAtivo,
                    TotalJogos = _context.Jogos.Count(j => j.Empresa.Id == e.Id && j.EstaAtivo),
                    MediaNotasJogos = _context.Avaliacoes
                        .Where(a => a.Jogo.Empresa.Id == e.Id && a.EstaAtivo)
                        .Average(a => (double?)a.Nota)
                })
                .OrderBy(e => e.NomeEmpresa)
                .ToListAsync();
        }

        public async Task<EmpresaDTO?> ObterEmpresaPorId(int id)
        {
            return await _context.Empresa
                .Where(e => e.Id == id && e.EstaAtivo)
                .Select(e => new EmpresaDTO
                {
                    EmpresaId = e.Id,
                    NomeEmpresa = e.NomeEmpresa,
                    EstaAtivo = e.EstaAtivo,
                    TotalJogos = _context.Jogos.Count(j => j.Empresa.Id == e.Id && j.EstaAtivo),
                    MediaNotasJogos = _context.Avaliacoes
                        .Where(a => a.Jogo.Empresa.Id == e.Id && a.EstaAtivo)
                        .Average(a => (double?)a.Nota)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<JogoDTO>> ListarJogosPorEmpresa(int empresaId)
        {
            return await _context.Jogos
                .Where(j => j.Empresa.Id == empresaId && j.EstaAtivo)
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
                .OrderByDescending(j => j.MediaAvaliacoes ?? 0)
                .ToListAsync();
        }
    }
}
