using AutoMapper;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Services
{
    public class AvaliacaoServices
    {
        private readonly GameLogContext _context;
        private readonly IMapper _mapper;

        public AvaliacaoServices(GameLogContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<Jogo> VerificarJogoExiste(int jogoId)
        {
            var jogo = await _context.Jogos.FindAsync(jogoId);
            if (jogo == null)
                throw new Exception("Jogo não encontrado");
            return jogo;
        }

        private async Task VerificarAvaliacaoDuplicada(int usuarioId, int jogoId)
        {
            var avaliacaoExistente = await _context.Avaliacoes
                .AnyAsync(a => a.Usuario.Id == usuarioId &&
                              a.Jogo.Id == jogoId &&
                              a.EstaAtivo);

            if (avaliacaoExistente)
                throw new Exception("Você já possui uma avaliação ativa para este jogo");
        }

        public async Task<AvaliacaoDTO> CriarAvaliacao(CriarAvaliacaoDTO avaliacaoDTO, int usuarioId)
        {
            await VerificarJogoExiste(avaliacaoDTO.JogoId);
            await VerificarAvaliacaoDuplicada(usuarioId, avaliacaoDTO.JogoId);

            var avaliacao = _mapper.Map<Avaliacao>(avaliacaoDTO);
            avaliacao.DataPublicacao = DateTime.UtcNow;
            avaliacao.EstaAtivo = true;
            avaliacao.Jogo = await _context.Jogos.FindAsync(avaliacaoDTO.JogoId);
            avaliacao.Usuario = await _context.Usuarios.FindAsync(usuarioId);

            _context.Avaliacoes.Add(avaliacao);
            await _context.SaveChangesAsync();

            return await ObterAvaliacaoDto(avaliacao.Id);
        }

        public async Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoes()
        {
            return await _context.Avaliacoes
                .Where(a => a.EstaAtivo)
                .OrderByDescending(a => a.DataPublicacao)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    JogoId = a.Jogo.Id,
                    NomeJogo = a.Jogo.Titulo,
                    NomeUsuario = a.Usuario.NomeUsuario,
                    TextoAvaliacao = a.TextoAvaliacao,
                    DataPublicacao = a.DataPublicacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count
                })
                .ToListAsync();
        }

        public async Task<AvaliacaoDTO?> ObterAvaliacaoPorId(int id)
        {
            return await _context.Avaliacoes
                .Where(a => a.Id == id && a.EstaAtivo)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    JogoId = a.Jogo.Id,
                    NomeJogo = a.Jogo.Titulo,
                    NomeUsuario = a.Usuario.NomeUsuario,
                    TextoAvaliacao = a.TextoAvaliacao,
                    DataPublicacao = a.DataPublicacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoesPorUsuario(int usuarioId)
        {
            return await _context.Avaliacoes
                .Where(a => a.Usuario.Id == usuarioId && a.EstaAtivo)
                .OrderByDescending(a => a.DataPublicacao)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    JogoId = a.Jogo.Id,
                    NomeJogo = a.Jogo.Titulo,
                    NomeUsuario = a.Usuario.NomeUsuario,
                    TextoAvaliacao = a.TextoAvaliacao,
                    DataPublicacao = a.DataPublicacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count
                })
                .ToListAsync();
        }

        public async Task<AvaliacaoDTO?> EditarAvaliacao(int id, EditarAvaliacaoDTO avaliacaoDTO, int usuarioId)
        {
            var avaliacao = await _context.Avaliacoes
                .FirstOrDefaultAsync(a => a.Id == id &&
                                        a.Usuario.Id == usuarioId &&
                                        a.EstaAtivo);

            if (avaliacao == null)
                return null;

            _mapper.Map(avaliacaoDTO, avaliacao);
            await _context.SaveChangesAsync();

            return await ObterAvaliacaoDto(avaliacao.Id);
        }

        public async Task<bool> DeletarAvaliacao(int id, int usuarioId)
        {
            var avaliacao = await _context.Avaliacoes
                .FirstOrDefaultAsync(a => a.Id == id &&
                                        a.Usuario.Id == usuarioId &&
                                        a.EstaAtivo);

            if (avaliacao == null)
                return false;

            avaliacao.EstaAtivo = false;
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<AvaliacaoDTO> ObterAvaliacaoDto(int id)
        {
            return await _context.Avaliacoes
                .Where(a => a.Id == id)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    JogoId = a.Jogo.Id,
                    NomeJogo = a.Jogo.Titulo,
                    NomeUsuario = a.Usuario.NomeUsuario,
                    TextoAvaliacao = a.TextoAvaliacao,
                    DataPublicacao = a.DataPublicacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count
                })
                .FirstAsync();
        }
    }
}