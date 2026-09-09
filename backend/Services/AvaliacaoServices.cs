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
                    ImagemJogo = a.Jogo.Imagem,
                    NomeEmpresa = a.Jogo.Empresa != null ? a.Jogo.Empresa.NomeEmpresa : null,
                    EmpresaId = a.Jogo.Empresa != null ? a.Jogo.Empresa.Id : (int?)null,
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
                    ImagemJogo = a.Jogo.Imagem,
                    NomeEmpresa = a.Jogo.Empresa != null ? a.Jogo.Empresa.NomeEmpresa : null,
                    EmpresaId = a.Jogo.Empresa != null ? a.Jogo.Empresa.Id : (int?)null,
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
                    ImagemJogo = a.Jogo.Imagem,
                    NomeEmpresa = a.Jogo.Empresa != null ? a.Jogo.Empresa.NomeEmpresa : null,
                    EmpresaId = a.Jogo.Empresa != null ? a.Jogo.Empresa.Id : (int?)null,
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

        public async Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoesPorJogo(int jogoId, int? usuarioId = null)
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
                    ImagemJogo = a.Jogo.Imagem,
                    NomeEmpresa = a.Jogo.Empresa != null ? a.Jogo.Empresa.NomeEmpresa : null,
                    EmpresaId = a.Jogo.Empresa != null ? a.Jogo.Empresa.Id : (int?)null,
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

            return await ObterAvaliacaoDto(avaliacao.Id, usuarioId);
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
                    ImagemJogo = a.Jogo.Imagem,
                    NomeEmpresa = a.Jogo.Empresa != null ? a.Jogo.Empresa.NomeEmpresa : null,
                    EmpresaId = a.Jogo.Empresa != null ? a.Jogo.Empresa.Id : (int?)null,
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

        public async Task<(bool Curtido, int TotalCurtidas)> AlternarCurtida(int avaliacaoId, int usuarioId)
        {
            var avaliacao = await _context.Avaliacoes
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.Id == avaliacaoId && a.EstaAtivo);

            if (avaliacao == null)
                throw new Exception("Avaliação não encontrada");

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

        public async Task<bool> AdicionarCurtida(int avaliacaoId, int usuarioId)
        {
            var avaliacao = await _context.Avaliacoes
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.Id == avaliacaoId && a.EstaAtivo);

            if (avaliacao == null) return false;
            if (avaliacao.Usuario.Id == usuarioId)
                throw new InvalidOperationException("Você não pode curtir sua própria avaliação.");

            var curtidaExistente = await _context.CurtidasDeAvaliacoes
                .FirstOrDefaultAsync(c => c.AvaliacaoId == avaliacaoId && c.UsuarioId == usuarioId);

            if (curtidaExistente != null)
            {
                if (curtidaExistente.EstaAtivo && curtidaExistente.Curtida) return false;
                curtidaExistente.EstaAtivo = true;
                curtidaExistente.Curtida = true;
            }
            else
            {
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario == null) return false;

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
                .FirstOrDefaultAsync(c => c.AvaliacaoId == avaliacaoId && c.UsuarioId == usuarioId && c.EstaAtivo && c.Curtida);

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

        // ======================= RESPOSTAS DE AVALIAÇÃO ======================= //

        public async Task<RespostaDeAvaliacaoDTO> AdicionarResposta(int avaliacaoId, int usuarioId, CriarRespostaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Comentario))
                throw new ArgumentException("O comentário não pode ser vazio.");

            var avaliacao = await _context.Avaliacoes.FirstOrDefaultAsync(a => a.Id == avaliacaoId && a.EstaAtivo);
            if (avaliacao == null)
                throw new Exception("Avaliação não encontrada");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId && u.EstaAtivo);
            if (usuario == null)
                throw new Exception("Usuário não encontrado");

            var resposta = new RespostaDeAvaliacao
            {
                AvaliacaoId = avaliacaoId,
                UsuarioId = usuarioId,
                Comentario = dto.Comentario.Trim(),
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

        public async Task<IEnumerable<RespostaDeAvaliacaoDTO>> ListarRespostasPorAvaliacao(int avaliacaoId, int? usuarioId = null)
        {
            return await _context.RespostasDeAvaliacao
                .Where(r => r.AvaliacaoId == avaliacaoId && r.EstaAtivo)
                .OrderBy(r => r.DataCriacao)
                .Select(r => new RespostaDeAvaliacaoDTO
                {
                    RespostaId = r.Id,
                    AvaliacaoId = r.AvaliacaoId ?? avaliacaoId,
                    UsuarioId = r.Usuario != null ? r.Usuario.Id : (r.UsuarioId ?? 0),
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

        public async Task<bool> DeletarResposta(int respostaId, int usuarioId)
        {
            var resposta = await _context.RespostasDeAvaliacao
                .FirstOrDefaultAsync(r => r.Id == respostaId && r.UsuarioId == usuarioId && r.EstaAtivo);

            if (resposta == null)
                return false;

            resposta.EstaAtivo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Curtido, int TotalCurtidas)> AlternarCurtidaResposta(int respostaId, int usuarioId)
        {
            var resposta = await _context.RespostasDeAvaliacao
                .FirstOrDefaultAsync(r => r.Id == respostaId && r.EstaAtivo);

            if (resposta == null)
                throw new Exception("Resposta não encontrada");

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
