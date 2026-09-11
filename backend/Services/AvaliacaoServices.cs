using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private async Task<Jogo> VerificarJogoExiste(Guid jogoId)
        {
            var jogo = await _context.Jogos.FindAsync(jogoId);
            if (jogo == null || !jogo.EstaAtivo)
                throw new KeyNotFoundException("Jogo não encontrado.");
            return jogo;
        }

        private async Task VerificarAvaliacaoDuplicada(Guid usuarioId, Guid jogoId)
        {
            var avaliacaoExistente = await _context.Avaliacoes
                .AnyAsync(a => a.Usuario.Id == usuarioId &&
                               a.Jogo.Id == jogoId &&
                               a.EstaAtivo);

            if (avaliacaoExistente)
                throw new InvalidOperationException("Você já possui uma avaliação ativa para este jogo.");
        }

        public async Task<AvaliacaoDTO> CriarAvaliacao(CriarAvaliacaoDTO avaliacaoDTO, Guid usuarioId)
        {
            if (avaliacaoDTO.Nota < 1 || avaliacaoDTO.Nota > 5)
            {
                throw new ArgumentException("A nota da avaliação deve estar entre 1 e 5 estrelas.");
            }

            var jogo = await VerificarJogoExiste(avaliacaoDTO.JogoId);
            await VerificarAvaliacaoDuplicada(usuarioId, avaliacaoDTO.JogoId);

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null || !usuario.EstaAtivo)
                throw new KeyNotFoundException("Usuário não encontrado.");

            var avaliacao = _mapper.Map<Avaliacao>(avaliacaoDTO);
            avaliacao.TextoAvaliacao = avaliacaoDTO.TextoAvaliacao?.Trim() ?? string.Empty;
            avaliacao.DataPublicacao = DateTime.UtcNow;
            avaliacao.EstaAtivo = true;
            avaliacao.Jogo = jogo;
            avaliacao.Usuario = usuario;

            _context.Avaliacoes.Add(avaliacao);
            await _context.SaveChangesAsync();

            return await ObterAvaliacaoDto(avaliacao.Id);
        }

        public async Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoes(Guid? usuarioId = null)
        {
            return await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.EstaAtivo)
                .OrderByDescending(a => a.DataPublicacao)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    JogoId = a.Jogo.Id,
                    NomeJogo = a.Jogo.Titulo,
                    ImagemJogo = a.Jogo.Imagem,
                    NomeEmpresa = a.Jogo.Empresa != null ? a.Jogo.Empresa.NomeEmpresa : null,
                    EmpresaId = a.Jogo.Empresa != null ? a.Jogo.Empresa.Id : (Guid?)null,
                    DataLancamentoJogo = a.Jogo.DataLancamento,
                    UsuarioId = a.Usuario.Id,
                    NomeUsuario = a.Usuario.NomeUsuario,
                    FotoPerfilUsuario = a.Usuario.FotoDePerfil,
                    TextoAvaliacao = a.TextoAvaliacao,
                    DataPublicacao = a.DataPublicacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count(c => c.EstaAtivo && c.Curtida),
                    CurtidaPorMim = usuarioId.HasValue && a.CurtidasDeAvaliacao.Any(c => c.UsuarioId == usuarioId.Value && c.EstaAtivo && c.Curtida),
                    TotalRespostas = a.RespostasDeAvaliacao.Count(r => r.EstaAtivo)
                })
                .ToListAsync();
        }

        public async Task<AvaliacaoDTO?> ObterAvaliacaoPorId(Guid id, Guid? usuarioId = null)
        {
            return await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Id == id && a.EstaAtivo)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    JogoId = a.Jogo.Id,
                    NomeJogo = a.Jogo.Titulo,
                    ImagemJogo = a.Jogo.Imagem,
                    NomeEmpresa = a.Jogo.Empresa != null ? a.Jogo.Empresa.NomeEmpresa : null,
                    EmpresaId = a.Jogo.Empresa != null ? a.Jogo.Empresa.Id : (Guid?)null,
                    DataLancamentoJogo = a.Jogo.DataLancamento,
                    UsuarioId = a.Usuario.Id,
                    NomeUsuario = a.Usuario.NomeUsuario,
                    FotoPerfilUsuario = a.Usuario.FotoDePerfil,
                    TextoAvaliacao = a.TextoAvaliacao,
                    DataPublicacao = a.DataPublicacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count(c => c.EstaAtivo && c.Curtida),
                    CurtidaPorMim = usuarioId.HasValue && a.CurtidasDeAvaliacao.Any(c => c.UsuarioId == usuarioId.Value && c.EstaAtivo && c.Curtida),
                    TotalRespostas = a.RespostasDeAvaliacao.Count(r => r.EstaAtivo)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoesPorUsuario(Guid usuarioId, Guid? usuarioSolicitanteId = null)
        {
            return await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Usuario.Id == usuarioId && a.EstaAtivo)
                .OrderByDescending(a => a.DataPublicacao)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    JogoId = a.Jogo.Id,
                    NomeJogo = a.Jogo.Titulo,
                    ImagemJogo = a.Jogo.Imagem,
                    NomeEmpresa = a.Jogo.Empresa != null ? a.Jogo.Empresa.NomeEmpresa : null,
                    EmpresaId = a.Jogo.Empresa != null ? a.Jogo.Empresa.Id : (Guid?)null,
                    DataLancamentoJogo = a.Jogo.DataLancamento,
                    UsuarioId = a.Usuario.Id,
                    NomeUsuario = a.Usuario.NomeUsuario,
                    FotoPerfilUsuario = a.Usuario.FotoDePerfil,
                    TextoAvaliacao = a.TextoAvaliacao,
                    DataPublicacao = a.DataPublicacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count(c => c.EstaAtivo && c.Curtida),
                    CurtidaPorMim = usuarioSolicitanteId.HasValue && a.CurtidasDeAvaliacao.Any(c => c.UsuarioId == usuarioSolicitanteId.Value && c.EstaAtivo && c.Curtida),
                    TotalRespostas = a.RespostasDeAvaliacao.Count(r => r.EstaAtivo)
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoesPorJogo(Guid jogoId, Guid? usuarioId = null)
        {
            return await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Jogo.Id == jogoId && a.EstaAtivo)
                .OrderByDescending(a => a.DataPublicacao)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    JogoId = a.Jogo.Id,
                    NomeJogo = a.Jogo.Titulo,
                    ImagemJogo = a.Jogo.Imagem,
                    NomeEmpresa = a.Jogo.Empresa != null ? a.Jogo.Empresa.NomeEmpresa : null,
                    EmpresaId = a.Jogo.Empresa != null ? a.Jogo.Empresa.Id : (Guid?)null,
                    DataLancamentoJogo = a.Jogo.DataLancamento,
                    UsuarioId = a.Usuario.Id,
                    NomeUsuario = a.Usuario.NomeUsuario,
                    FotoPerfilUsuario = a.Usuario.FotoDePerfil,
                    TextoAvaliacao = a.TextoAvaliacao,
                    DataPublicacao = a.DataPublicacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count(c => c.EstaAtivo && c.Curtida),
                    CurtidaPorMim = usuarioId.HasValue && a.CurtidasDeAvaliacao.Any(c => c.UsuarioId == usuarioId.Value && c.EstaAtivo && c.Curtida),
                    TotalRespostas = a.RespostasDeAvaliacao.Count(r => r.EstaAtivo)
                })
                .ToListAsync();
        }

        public async Task<AvaliacaoDTO?> EditarAvaliacao(Guid id, EditarAvaliacaoDTO avaliacaoDTO, Guid usuarioId)
        {
            var avaliacao = await _context.Avaliacoes
                .FirstOrDefaultAsync(a => a.Id == id &&
                                          a.Usuario.Id == usuarioId &&
                                          a.EstaAtivo);

            if (avaliacao == null)
                return null;

            if (avaliacaoDTO.Nota < 1 || avaliacaoDTO.Nota > 5)
            {
                throw new ArgumentException("A nota da avaliação deve estar entre 1 e 5 estrelas.");
            }
            avaliacao.Nota = avaliacaoDTO.Nota;

            if (avaliacaoDTO.TextoAvaliacao != null)
            {
                avaliacao.TextoAvaliacao = avaliacaoDTO.TextoAvaliacao.Trim();
            }

            await _context.SaveChangesAsync();

            return await ObterAvaliacaoDto(avaliacao.Id, usuarioId);
        }

        public async Task<bool> DeletarAvaliacao(Guid id, Guid usuarioId)
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

        private async Task<AvaliacaoDTO> ObterAvaliacaoDto(Guid id, Guid? usuarioId = null)
        {
            return await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    JogoId = a.Jogo.Id,
                    NomeJogo = a.Jogo.Titulo,
                    ImagemJogo = a.Jogo.Imagem,
                    NomeEmpresa = a.Jogo.Empresa != null ? a.Jogo.Empresa.NomeEmpresa : null,
                    EmpresaId = a.Jogo.Empresa != null ? a.Jogo.Empresa.Id : (Guid?)null,
                    DataLancamentoJogo = a.Jogo.DataLancamento,
                    UsuarioId = a.Usuario.Id,
                    NomeUsuario = a.Usuario.NomeUsuario,
                    FotoPerfilUsuario = a.Usuario.FotoDePerfil,
                    TextoAvaliacao = a.TextoAvaliacao,
                    DataPublicacao = a.DataPublicacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count(c => c.EstaAtivo && c.Curtida),
                    CurtidaPorMim = usuarioId.HasValue && a.CurtidasDeAvaliacao.Any(c => c.UsuarioId == usuarioId.Value && c.EstaAtivo && c.Curtida),
                    TotalRespostas = a.RespostasDeAvaliacao.Count(r => r.EstaAtivo)
                })
                .FirstAsync();
        }

        public async Task<(bool Curtido, int TotalCurtidas)> AlternarCurtida(Guid avaliacaoId, Guid usuarioId)
        {
            var avaliacao = await _context.Avaliacoes
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.Id == avaliacaoId && a.EstaAtivo);

            if (avaliacao == null)
                throw new KeyNotFoundException("Avaliação não encontrada.");

            if (avaliacao.Usuario.Id == usuarioId)
                throw new InvalidOperationException("Você não pode curtir sua própria avaliação.");

            var curtidaExistente = await _context.CurtidasDeAvaliacoes
                .FirstOrDefaultAsync(c => c.AvaliacaoId == avaliacaoId && c.UsuarioId == usuarioId);

            bool novoEstadoCurtida;
            if (curtidaExistente == null)
            {
                _context.CurtidasDeAvaliacoes.Add(new CurtidaDeAvaliacao
                {
                    AvaliacaoId = avaliacaoId,
                    UsuarioId = usuarioId,
                    Curtida = true,
                    EstaAtivo = true
                });
                novoEstadoCurtida = true;
            }
            else
            {
                novoEstadoCurtida = !curtidaExistente.Curtida || !curtidaExistente.EstaAtivo;
                curtidaExistente.Curtida = novoEstadoCurtida;
                curtidaExistente.EstaAtivo = novoEstadoCurtida;
            }

            await _context.SaveChangesAsync();
            var total = await ContarCurtidas(avaliacaoId);
            return (novoEstadoCurtida, total);
        }

        public async Task<bool> RemoverCurtida(Guid avaliacaoId, Guid usuarioId)
        {
            var curtida = await _context.CurtidasDeAvaliacoes
                .FirstOrDefaultAsync(c => c.AvaliacaoId == avaliacaoId && c.UsuarioId == usuarioId && c.EstaAtivo && c.Curtida);

            if (curtida == null)
                return false;

            curtida.EstaAtivo = false;
            curtida.Curtida = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> ContarCurtidas(Guid avaliacaoId)
        {
            return await _context.CurtidasDeAvaliacoes
                .CountAsync(c => c.AvaliacaoId == avaliacaoId && c.Curtida && c.EstaAtivo);
        }

        public async Task<bool> UsuarioCurtiu(Guid avaliacaoId, Guid usuarioId)
        {
            return await _context.CurtidasDeAvaliacoes
                .AnyAsync(c => c.AvaliacaoId == avaliacaoId && c.UsuarioId == usuarioId && c.Curtida && c.EstaAtivo);
        }

        // ======================= RESPOSTAS DE AVALIAÇÃO ======================= //

        public async Task<RespostaDeAvaliacaoDTO> AdicionarResposta(Guid avaliacaoId, Guid usuarioId, CriarRespostaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Comentario))
                throw new ArgumentException("O comentário não pode ser vazio.");

            var comentario = dto.Comentario.Trim();
            if (comentario.Length > 500)
            {
                throw new ArgumentException("O comentário pode ter no máximo 500 caracteres.");
            }

            var avaliacao = await _context.Avaliacoes.FirstOrDefaultAsync(a => a.Id == avaliacaoId && a.EstaAtivo);
            if (avaliacao == null)
                throw new KeyNotFoundException("Avaliação não encontrada.");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId && u.EstaAtivo);
            if (usuario == null)
                throw new KeyNotFoundException("Usuário não encontrado.");

            var resposta = new RespostaDeAvaliacao
            {
                AvaliacaoId = avaliacaoId,
                UsuarioId = usuarioId,
                Comentario = comentario,
                DataCriacao = DateTime.UtcNow,
                EstaAtivo = true
            };

            _context.RespostasDeAvaliacao.Add(resposta);
            await _context.SaveChangesAsync();

            return new RespostaDeAvaliacaoDTO
            {
                RespostaId = resposta.Id,
                AvaliacaoId = avaliacaoId,
                UsuarioId = usuario.Id,
                NomeUsuario = usuario.NomeUsuario,
                FotoPerfilUsuario = usuario.FotoDePerfil,
                Comentario = resposta.Comentario,
                DataCriacao = resposta.DataCriacao,
                MinhaResposta = true,
                TotalCurtidas = 0,
                CurtidaPorMim = false
            };
        }

        public async Task<IEnumerable<RespostaDeAvaliacaoDTO>> ListarRespostasPorAvaliacao(Guid avaliacaoId, Guid? usuarioId = null)
        {
            return await _context.RespostasDeAvaliacao
                .AsNoTracking()
                .Where(r => r.AvaliacaoId == avaliacaoId && r.EstaAtivo)
                .OrderBy(r => r.DataCriacao)
                .Select(r => new RespostaDeAvaliacaoDTO
                {
                    RespostaId = r.Id,
                    AvaliacaoId = r.AvaliacaoId ?? avaliacaoId,
                    UsuarioId = r.Usuario != null ? r.Usuario.Id : (r.UsuarioId ?? Guid.Empty),
                    NomeUsuario = r.Usuario != null ? r.Usuario.NomeUsuario : "Gamer",
                    FotoPerfilUsuario = r.Usuario != null ? r.Usuario.FotoDePerfil : null,
                    Comentario = r.Comentario,
                    DataCriacao = r.DataCriacao,
                    MinhaResposta = usuarioId.HasValue && r.UsuarioId == usuarioId.Value,
                    TotalCurtidas = r.CurtidasDeRespostas.Count(c => c.EstaAtivo && c.Curtida),
                    CurtidaPorMim = usuarioId.HasValue && r.CurtidasDeRespostas.Any(c => c.UsuarioId == usuarioId.Value && c.EstaAtivo && c.Curtida)
                })
                .ToListAsync();
        }

        public async Task<bool> DeletarResposta(Guid respostaId, Guid usuarioId)
        {
            var resposta = await _context.RespostasDeAvaliacao
                .Include(r => r.Avaliacao)
                    .ThenInclude(a => a.Usuario)
                .FirstOrDefaultAsync(r => r.Id == respostaId && r.EstaAtivo);

            if (resposta == null)
                return false;

            // Permitir se for o autor do comentário OU o dono da avaliação (poder de moderação)
            var isAutorResposta = resposta.UsuarioId == usuarioId;
            var isDonoAvaliacao = resposta.Avaliacao != null && resposta.Avaliacao.Usuario != null && resposta.Avaliacao.Usuario.Id == usuarioId;

            if (!isAutorResposta && !isDonoAvaliacao)
            {
                return false;
            }

            resposta.EstaAtivo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Curtido, int TotalCurtidas)> AlternarCurtidaResposta(Guid respostaId, Guid usuarioId)
        {
            var resposta = await _context.RespostasDeAvaliacao
                .FirstOrDefaultAsync(r => r.Id == respostaId && r.EstaAtivo);

            if (resposta == null)
                throw new KeyNotFoundException("Resposta não encontrada.");

            if (resposta.UsuarioId == usuarioId)
                throw new InvalidOperationException("Você não pode curtir seu próprio comentário.");

            var curtidaExistente = await _context.CurtidasDeRespostas
                .FirstOrDefaultAsync(c => c.RespostaDeAvaliacaoId == respostaId && c.UsuarioId == usuarioId);

            bool novoEstado;
            if (curtidaExistente == null)
            {
                _context.CurtidasDeRespostas.Add(new CurtidaDeResposta
                {
                    RespostaDeAvaliacaoId = respostaId,
                    UsuarioId = usuarioId,
                    Curtida = true,
                    EstaAtivo = true
                });
                novoEstado = true;
            }
            else
            {
                novoEstado = !curtidaExistente.Curtida || !curtidaExistente.EstaAtivo;
                curtidaExistente.Curtida = novoEstado;
                curtidaExistente.EstaAtivo = novoEstado;
            }

            await _context.SaveChangesAsync();
            var total = await _context.CurtidasDeRespostas.CountAsync(c => c.RespostaDeAvaliacaoId == respostaId && c.EstaAtivo && c.Curtida);
            return (novoEstado, total);
        }
    }
}
