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
            if (jogo == null || !jogo.EstaAtivo)
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
            var jogo = await VerificarJogoExiste(avaliacaoDTO.JogoId);
            await VerificarAvaliacaoDuplicada(usuarioId, avaliacaoDTO.JogoId);

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null || !usuario.EstaAtivo)
                throw new Exception("Usuário não encontrado");

            var avaliacao = _mapper.Map<Avaliacao>(avaliacaoDTO);
            avaliacao.DataPublicacao = DateTime.UtcNow;
            avaliacao.EstaAtivo = true;
            avaliacao.Jogo = jogo;
            avaliacao.Usuario = usuario;

            _context.Avaliacoes.Add(avaliacao);
            await _context.SaveChangesAsync();

            return await ObterAvaliacaoDto(avaliacao.Id);
        }

        public async Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoes(int? usuarioId = null)
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
                    DataPublicacao = a.DataPublicacao
                })
                .ToListAsync();
        }

        public async Task<AvaliacaoDTO?> ObterAvaliacaoPorId(int id, int? usuarioId = null)
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
                    DataPublicacao = a.DataPublicacao
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoesPorUsuario(int usuarioId, int? usuarioSolicitanteId = null)
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
                    DataPublicacao = a.DataPublicacao
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoesPorJogo(int jogoId)
        {
            return await _context.Avaliacoes
                .Where(a => a.Jogo.Id == jogoId && a.EstaAtivo)
                .OrderByDescending(a => a.DataPublicacao)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    JogoId = a.Jogo.Id,
                    NomeJogo = a.Jogo.Titulo,
                    NomeUsuario = a.Usuario.NomeUsuario,
                    TextoAvaliacao = a.TextoAvaliacao,
                    DataPublicacao = a.DataPublicacao
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

        private async Task<AvaliacaoDTO> ObterAvaliacaoDto(int id, int? usuarioId = null)
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
                    DataPublicacao = a.DataPublicacao
                })
                .FirstAsync();
        }

        public async Task<bool> AdicionarCurtida(int avaliacaoId, int usuarioId)
        {
            var curtidaExistente = await _context.CurtidasDeAvaliacoes
                .FirstOrDefaultAsync(c => c.AvaliacaoId == avaliacaoId && c.UsuarioId == usuarioId);

            if (curtidaExistente != null)
            {
                if (curtidaExistente.EstaAtivo) return false;
                curtidaExistente.EstaAtivo = true;
                curtidaExistente.Curtida = true;
            }
            else
            {
                var avaliacao = await _context.Avaliacoes.FindAsync(avaliacaoId);
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (avaliacao == null || usuario == null) return false;

                _context.CurtidasDeAvaliacoes.Add(new CurtidaDeAvaliacao
                {
                    AvaliacaoId = avaliacaoId,
                    UsuarioId = usuarioId,
                    Curtida = true,
                    EstaAtivo = true
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoverCurtida(int avaliacaoId, int usuarioId)
        {
            var curtida = await _context.CurtidasDeAvaliacoes
                .FirstOrDefaultAsync(c => c.AvaliacaoId == avaliacaoId && c.UsuarioId == usuarioId && c.EstaAtivo);

            if (curtida == null)
                return false;

            curtida.EstaAtivo = false;
            curtida.Curtida = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ContarCurtidas(int avaliacaoId)
        {
            return await _context.CurtidasDeAvaliacoes
                .CountAsync(c => c.AvaliacaoId == avaliacaoId && c.Curtida && c.EstaAtivo);
        }

        public async Task<bool> UsuarioCurtiu(int avaliacaoId, int usuarioId)
        {
            return await _context.CurtidasDeAvaliacoes
                .AnyAsync(c => c.AvaliacaoId == avaliacaoId && c.UsuarioId == usuarioId && c.Curtida && c.EstaAtivo);
        }
    }
}
