using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using GameLog_Backend.Configurations;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GameLog_Backend.Services
{
    public class UsuarioServices
    {
        private readonly GameLogContext _context;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;

        public UsuarioServices(GameLogContext context, IMapper mapper, IOptions<JwtSettings> jwtOptions)
        {
            _context = context;
            _mapper = mapper;
            _jwtSettings = jwtOptions.Value ?? new JwtSettings
            {
                Key = "GameLogSuperSecretKeyDefault1234567890!SecureLongKey256Bit",
                Issuer = "GameLogAPI",
                Audience = "GameLogClient",
                ExpireHours = 24
            };
        }

        private void ValidarEmailESenha(string email, string? senha, string? nomeUsuario = null)
        {
            if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                throw new ArgumentException("Informe um endereço de e-mail válido.");
            }

            if (!string.IsNullOrEmpty(senha))
            {
                if (senha.Length < 6)
                {
                    throw new ArgumentException("A senha deve ter no mínimo 6 caracteres.");
                }

                if (!Regex.IsMatch(senha, @"[A-Z]"))
                {
                    throw new ArgumentException("A senha deve conter pelo menos uma letra maiúscula.");
                }

                if (!Regex.IsMatch(senha, @"[0-9]"))
                {
                    throw new ArgumentException("A senha deve conter pelo menos um número.");
                }
            }

            if (nomeUsuario != null)
            {
                var trimmed = nomeUsuario.Trim();
                if (trimmed.Length < 3 || trimmed.Length > 50)
                {
                    throw new ArgumentException("O nome de usuário deve ter entre 3 e 50 caracteres.");
                }

                if (!Regex.IsMatch(trimmed, @"^[a-zA-Z0-9_\.]+$"))
                {
                    throw new ArgumentException("O nome de usuário pode conter apenas letras, números, ponto (.) e sublinhado (_).");
                }
            }
        }

        public async Task<(UsuarioDTO? usuario, string? token, DateTime expiraEm)> AutenticarUsuario(UsuarioLoginDTO loginDTO)
        {
            if (string.IsNullOrWhiteSpace(loginDTO.Email) || string.IsNullOrWhiteSpace(loginDTO.Senha))
            {
                return (null, null, DateTime.MinValue);
            }

            var email = loginDTO.Email.Trim().ToLower();
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email && u.EstaAtivo);

            if (usuario == null || !VerificarSenha(loginDTO.Senha, usuario.Senha))
                return (null, null, DateTime.MinValue);

            var usuarioDTO = _mapper.Map<UsuarioDTO>(usuario);
            var token = GerarTokenJwt(usuario);
            var expiraEm = DateTime.UtcNow.AddHours(_jwtSettings.ExpireHours);

            return (usuarioDTO, token, expiraEm);
        }

        private string GerarTokenJwt(Usuario usuario)
        {
            var keyString = !string.IsNullOrEmpty(_jwtSettings.Key) && _jwtSettings.Key != "{JWT_SECRET}"
                ? _jwtSettings.Key
                : "GameLogSuperSecretKeyDefault1234567890!SecureLongKey256Bit";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()), 
                new Claim("nomeUsuario", usuario.NomeUsuario), 
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpireHours), 
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<IEnumerable<UsuarioDTO>> ListarUsuarios()
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.EstaAtivo)
                .Select(u => _mapper.Map<UsuarioDTO>(u))
                .ToListAsync();
        }

        public async Task<UsuarioDTO?> ObterUsuarioPorId(int id)
        {
            var usuario = await _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && u.EstaAtivo);

            return usuario != null ? _mapper.Map<UsuarioDTO>(usuario) : null;
        }

        public async Task<UsuarioDTO> CriarUsuario(CriarUsuarioDTO usuarioDTO)
        {
            usuarioDTO.Email = usuarioDTO.Email?.Trim() ?? string.Empty;
            usuarioDTO.NomeUsuario = usuarioDTO.NomeUsuario?.Trim() ?? string.Empty;

            ValidarEmailESenha(usuarioDTO.Email, usuarioDTO.Senha, usuarioDTO.NomeUsuario);

            if (await EmailEmUso(usuarioDTO.Email))
            {
                throw new InvalidOperationException("Este e-mail já está em uso por outra conta.");
            }

            if (await NomeUsuarioEmUso(usuarioDTO.NomeUsuario))
            {
                throw new InvalidOperationException("Este nome de usuário já está em uso.");
            }

            var usuario = _mapper.Map<Usuario>(usuarioDTO);
            usuario.Senha = HashSenha(usuarioDTO.Senha);
            usuario.EstaAtivo = true;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return _mapper.Map<UsuarioDTO>(usuario);
        }

        private string HashSenha(string senha)
        {
            return BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 11);
        }

        private bool VerificarSenha(string senha, string senhaHash)
        {
            if (string.IsNullOrEmpty(senha) || string.IsNullOrEmpty(senhaHash))
                return false;

            try
            {
                if (senhaHash.StartsWith("$2"))
                {
                    return BCrypt.Net.BCrypt.Verify(senha, senhaHash);
                }
                
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
                var legacyHash = Convert.ToBase64String(bytes);
                return legacyHash == senhaHash;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> EmailEmUso(string email)
        {
            var emailLimpo = email.Trim().ToLower();
            return await _context.Usuarios
                .AnyAsync(u => u.Email.ToLower() == emailLimpo && u.EstaAtivo);
        }

        public async Task<bool> NomeUsuarioEmUso(string nomeUsuario)
        {
            var nomeLimpo = nomeUsuario.Trim().ToLower();
            return await _context.Usuarios
                .AnyAsync(u => u.NomeUsuario.ToLower() == nomeLimpo && u.EstaAtivo);
        }

        public async Task<UsuarioDTO?> EditarUsuario(int id, string senhaAtual, EditarUsuarioDTO usuarioDTO)
        {
            var usuarioExistente = await _context.Usuarios.FindAsync(id);
            if (usuarioExistente == null || !usuarioExistente.EstaAtivo || !VerificarSenha(senhaAtual, usuarioExistente.Senha))
            {
                return null;
            }

            // Sanitização de entradas
            usuarioDTO.Email = usuarioDTO.Email?.Trim() ?? usuarioExistente.Email;
            usuarioDTO.NomeUsuario = usuarioDTO.NomeUsuario?.Trim() ?? usuarioExistente.NomeUsuario;
            if (usuarioDTO.Bio != null)
            {
                usuarioDTO.Bio = usuarioDTO.Bio.Trim();
                if (usuarioDTO.Bio.Length > 300)
                {
                    usuarioDTO.Bio = usuarioDTO.Bio.Substring(0, 300);
                }
            }

            ValidarEmailESenha(usuarioDTO.Email, usuarioDTO.NovaSenha, usuarioDTO.NomeUsuario);

            if (usuarioDTO.Email.ToLower() != usuarioExistente.Email.ToLower() && await EmailEmUso(usuarioDTO.Email))
            {
                throw new InvalidOperationException("O novo e-mail já está em uso por outro usuário.");
            }

            if (usuarioDTO.NomeUsuario.ToLower() != usuarioExistente.NomeUsuario.ToLower() && await NomeUsuarioEmUso(usuarioDTO.NomeUsuario))
            {
                throw new InvalidOperationException("O novo nome de usuário já está em uso.");
            }

            _mapper.Map(usuarioDTO, usuarioExistente);

            // Atualização segura de senha: apenas se NovaSenha foi expressamente fornecida
            if (!string.IsNullOrWhiteSpace(usuarioDTO.NovaSenha))
            {
                usuarioExistente.Senha = HashSenha(usuarioDTO.NovaSenha);
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<UsuarioDTO>(usuarioExistente);
        }

        public async Task<bool> DeletarUsuario(int id, string senhaAtual)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null || !usuario.EstaAtivo || !VerificarSenha(senhaAtual, usuario.Senha))
            {
                return false;
            }

            usuario.EstaAtivo = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<GeneroFavoritoDTO>> IdentificaTopNGenerosFavoritos(int id, int topN = 3)
        {
            var avaliacoesDoUsuario = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Usuario.Id == id && a.EstaAtivo)
                .Include(a => a.Jogo)
                    .ThenInclude(j => j.Generos) 
                .ToListAsync(); 

            if (!avaliacoesDoUsuario.Any())
            {
                return new List<GeneroFavoritoDTO>();
            }

            var generosComNotas = avaliacoesDoUsuario
                .Where(a => a.Jogo != null && a.Jogo.Generos != null)
                .SelectMany(a => a.Jogo.Generos.Select(g => new { Genero = g.TituloGenero, Nota = a.Nota }));

            var topGeneros = generosComNotas
                .GroupBy(x => x.Genero)
                .Select(g => new
                {
                    Genero = g.Key,
                    MediaNotas = g.Average(x => (double)x.Nota), 
                    QuantidadeJogos = g.Count() 
                })
                .OrderByDescending(x => x.MediaNotas)
                .ThenByDescending(x => x.QuantidadeJogos)
                .Take(topN) 
                .ToList();

             return topGeneros.Select(g => new GeneroFavoritoDTO { Genero = g.Genero }).ToList();
        }

        public async Task<IEnumerable<JogoRecomendacaoDTO>> RecomendarJogos(int usuarioId)
        {
            var topGeneros = await IdentificaTopNGenerosFavoritos(usuarioId, 3);

            if (!topGeneros.Any())
            {
                return Enumerable.Empty<JogoRecomendacaoDTO>();
            }

            var jogosAvaliadosIds = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Usuario.Id == usuarioId && a.EstaAtivo)
                .Select(a => a.Jogo.Id)
                .ToListAsync();

            var generosParaBuscar = topGeneros.Select(g => g.Genero).ToList();

            var jogosCandidatos = await _context.Jogos
                .AsNoTracking()
                .Include(j => j.Generos)
                .Where(j => j.EstaAtivo && !jogosAvaliadosIds.Contains(j.Id) &&
                            j.Generos.Any(g => generosParaBuscar.Contains(g.TituloGenero)))
                .OrderByDescending(j => j.DataLancamento)
                .Take(10)
                .ToListAsync();

            var recomendados = jogosCandidatos.Select(j =>
            {
                var generoCorrespondente = j.Generos.FirstOrDefault(g => generosParaBuscar.Contains(g.TituloGenero))?.TituloGenero
                                          ?? topGeneros.First().Genero;
                return new JogoRecomendacaoDTO
                {
                    JogoId = j.Id,
                    Titulo = j.Titulo,
                    Descricao = j.Descricao,
                    Imagem = j.Imagem,
                    DataLancamento = j.DataLancamento,
                    GeneroFavorito = generoCorrespondente
                };
            }).ToList();

            return recomendados;
        }

        // ======================= SISTEMA SOCIAL (SEGUIR & FEED) ======================= //

        public async Task<(bool Seguido, int TotalSeguidores)> AlternarSeguirUsuario(int seguidorId, int seguidoId)
        {
            if (seguidorId == seguidoId)
            {
                throw new InvalidOperationException("Você não pode seguir seu próprio perfil.");
            }

            var seguido = await _context.Usuarios.FindAsync(seguidoId);
            if (seguido == null || !seguido.EstaAtivo)
            {
                throw new KeyNotFoundException("Usuário não encontrado.");
            }

            var seguidor = await _context.Usuarios.FindAsync(seguidorId);
            if (seguidor == null || !seguidor.EstaAtivo)
            {
                throw new KeyNotFoundException("Usuário autenticado não encontrado.");
            }

            var relacaoExistente = await _context.SegueUsuarios
                .Include(s => s.UsuarioSeguidor)
                .Include(s => s.UsuarioSeguido)
                .FirstOrDefaultAsync(s => s.UsuarioSeguidor.Id == seguidorId && s.UsuarioSeguido.Id == seguidoId);

            bool novoEstado;
            if (relacaoExistente == null)
            {
                _context.SegueUsuarios.Add(new SegueUsuario
                {
                    UsuarioSeguidor = seguidor,
                    UsuarioSeguido = seguido,
                    EstaAtivo = true
                });
                novoEstado = true;
            }
            else
            {
                novoEstado = !relacaoExistente.EstaAtivo;
                relacaoExistente.EstaAtivo = novoEstado;
            }

            await _context.SaveChangesAsync();

            var totalSeguidores = await _context.SegueUsuarios
                .CountAsync(s => s.UsuarioSeguido.Id == seguidoId && s.EstaAtivo);

            return (novoEstado, totalSeguidores);
        }

        public async Task<bool> VerificarSeSegue(int seguidorId, int seguidoId)
        {
            if (seguidorId == seguidoId) return false;

            return await _context.SegueUsuarios
                .AnyAsync(s => s.UsuarioSeguidor.Id == seguidorId && s.UsuarioSeguido.Id == seguidoId && s.EstaAtivo);
        }

        public async Task<EstatisticasSociaisDTO> ObterEstatisticasSociais(int usuarioId, int? solicitanteId = null)
        {
            var totalSeguidores = await _context.SegueUsuarios
                .CountAsync(s => s.UsuarioSeguido.Id == usuarioId && s.EstaAtivo);

            var totalSeguindo = await _context.SegueUsuarios
                .CountAsync(s => s.UsuarioSeguidor.Id == usuarioId && s.EstaAtivo);

            var seguidoPorMim = solicitanteId.HasValue && await VerificarSeSegue(solicitanteId.Value, usuarioId);

            return new EstatisticasSociaisDTO
            {
                TotalSeguidores = totalSeguidores,
                TotalSeguindo = totalSeguindo,
                SeguidoPorMim = seguidoPorMim
            };
        }

        public async Task<List<UsuarioConexaoDTO>> ObterSeguidores(int usuarioId, int? solicitanteId = null)
        {
            var conexoes = await _context.SegueUsuarios
                .AsNoTracking()
                .Include(s => s.UsuarioSeguidor)
                .Where(s => s.UsuarioSeguido.Id == usuarioId && s.EstaAtivo && s.UsuarioSeguidor.EstaAtivo)
                .ToListAsync();

            var seguidorIds = conexoes.Select(c => c.UsuarioSeguidor.Id).Distinct().ToList();

            var seguidosPeloSolicitante = new HashSet<int>();
            if (solicitanteId.HasValue)
            {
                seguidosPeloSolicitante = (await _context.SegueUsuarios
                    .AsNoTracking()
                    .Where(s => s.UsuarioSeguidor.Id == solicitanteId.Value && seguidorIds.Contains(s.UsuarioSeguido.Id) && s.EstaAtivo)
                    .Select(s => s.UsuarioSeguido.Id)
                    .ToListAsync())
                    .ToHashSet();
            }

            return conexoes.Select(c => new UsuarioConexaoDTO
            {
                UsuarioId = c.UsuarioSeguidor.Id,
                NomeUsuario = c.UsuarioSeguidor.NomeUsuario,
                FotoPerfil = c.UsuarioSeguidor.FotoDePerfil,
                Bio = c.UsuarioSeguidor.Bio,
                SeguidoPorMim = solicitanteId.HasValue && seguidosPeloSolicitante.Contains(c.UsuarioSeguidor.Id)
            }).ToList();
        }

        public async Task<List<UsuarioConexaoDTO>> ObterSeguindo(int usuarioId, int? solicitanteId = null)
        {
            var conexoes = await _context.SegueUsuarios
                .AsNoTracking()
                .Include(s => s.UsuarioSeguido)
                .Where(s => s.UsuarioSeguidor.Id == usuarioId && s.EstaAtivo && s.UsuarioSeguido.EstaAtivo)
                .ToListAsync();

            var seguidoIds = conexoes.Select(c => c.UsuarioSeguido.Id).Distinct().ToList();

            var seguidosPeloSolicitante = new HashSet<int>();
            if (solicitanteId.HasValue)
            {
                seguidosPeloSolicitante = (await _context.SegueUsuarios
                    .AsNoTracking()
                    .Where(s => s.UsuarioSeguidor.Id == solicitanteId.Value && seguidoIds.Contains(s.UsuarioSeguido.Id) && s.EstaAtivo)
                    .Select(s => s.UsuarioSeguido.Id)
                    .ToListAsync())
                    .ToHashSet();
            }

            return conexoes.Select(c => new UsuarioConexaoDTO
            {
                UsuarioId = c.UsuarioSeguido.Id,
                NomeUsuario = c.UsuarioSeguido.NomeUsuario,
                FotoPerfil = c.UsuarioSeguido.FotoDePerfil,
                Bio = c.UsuarioSeguido.Bio,
                SeguidoPorMim = solicitanteId.HasValue && seguidosPeloSolicitante.Contains(c.UsuarioSeguido.Id)
            }).ToList();
        }

        public async Task<List<ItemFeedSocialDTO>> ObterFeedSocial(int usuarioId, int pagina = 1, int itensPorPagina = 20)
        {
            pagina = Math.Max(1, pagina);
            itensPorPagina = Math.Clamp(itensPorPagina, 1, 50);

            var seguindoIds = await _context.SegueUsuarios
                .AsNoTracking()
                .Where(s => s.UsuarioSeguidor.Id == usuarioId && s.EstaAtivo)
                .Select(s => s.UsuarioSeguido.Id)
                .ToListAsync();

            if (!seguindoIds.Any())
            {
                return new List<ItemFeedSocialDTO>();
            }

            // 1. Avaliações postadas pelos seguidos
            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Include(a => a.Usuario)
                .Include(a => a.Jogo)
                    .ThenInclude(j => j.Empresa)
                .Where(a => seguindoIds.Contains(a.Usuario.Id) && a.EstaAtivo)
                .OrderByDescending(a => a.DataPublicacao)
                .Take(itensPorPagina * 2)
                .Select(a => new ItemFeedSocialDTO
                {
                    Id = "eval-" + a.Id,
                    TipoAtividade = "Avaliacao",
                    DataAtividade = a.DataPublicacao,
                    AutorId = a.Usuario.Id,
                    AutorNome = a.Usuario.NomeUsuario,
                    AutorFoto = a.Usuario.FotoDePerfil,
                    JogoId = a.Jogo.Id,
                    JogoTitulo = a.Jogo.Titulo,
                    JogoImagem = a.Jogo.Imagem,
                    NomeEmpresa = a.Jogo.Empresa != null ? a.Jogo.Empresa.NomeEmpresa : null,
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    TextoAvaliacao = a.TextoAvaliacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count(c => c.EstaAtivo && c.Curtida),
                    CurtidaPorMim = a.CurtidasDeAvaliacao.Any(c => c.UsuarioId == usuarioId && c.EstaAtivo && c.Curtida),
                    TotalRespostas = a.RespostasDeAvaliacao.Count(r => r.EstaAtivo)
                })
                .ToListAsync();

            // 2. Jogos Zerados adicionados à biblioteca
            var zerados = await _context.ItensBiblioteca
                .AsNoTracking()
                .Include(b => b.Jogo)
                    .ThenInclude(j => j.Empresa)
                .Where(b => seguindoIds.Contains(b.UsuarioId) && b.Status == StatusJogo.Zerado && b.EstaAtivo)
                .OrderByDescending(b => b.DataAtualizacao)
                .Take(itensPorPagina * 2)
                .ToListAsync();

            var zeradosUserIds = zerados.Select(z => z.UsuarioId).Distinct().ToList();
            var usuariosMap = await _context.Usuarios
                .AsNoTracking()
                .Where(u => zeradosUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => new { u.NomeUsuario, u.FotoDePerfil });

            var zeradosItems = zerados.Select(z =>
            {
                usuariosMap.TryGetValue(z.UsuarioId, out var u);
                return new ItemFeedSocialDTO
                {
                    Id = "zerado-" + z.Id,
                    TipoAtividade = "JogoZerado",
                    DataAtividade = z.DataConclusao ?? z.DataAtualizacao,
                    AutorId = z.UsuarioId,
                    AutorNome = u?.NomeUsuario ?? "Gamer",
                    AutorFoto = u?.FotoDePerfil,
                    JogoId = z.JogoId,
                    JogoTitulo = z.Jogo.Titulo,
                    JogoImagem = z.Jogo.Imagem,
                    NomeEmpresa = z.Jogo.Empresa?.NomeEmpresa
                };
            }).ToList();

            // 3. Listas públicas criadas pelos seguidos
            var listas = await _context.ListasDeJogos
                .AsNoTracking()
                .Include(l => l.Usuario)
                .Include(l => l.Itens)
                    .ThenInclude(i => i.Jogo)
                .Where(l => seguindoIds.Contains(l.UsuarioId) && l.EstaPublica && l.EstaAtivo)
                .OrderByDescending(l => l.DataCriacao)
                .Take(itensPorPagina * 2)
                .ToListAsync();

            var listasItems = listas.Select(l => new ItemFeedSocialDTO
            {
                Id = "lista-" + l.Id,
                TipoAtividade = "ListaCriada",
                DataAtividade = l.DataCriacao,
                AutorId = l.UsuarioId,
                AutorNome = l.Usuario.NomeUsuario,
                AutorFoto = l.Usuario.FotoDePerfil,
                ListaId = l.Id,
                ListaTitulo = l.Titulo,
                ListaDescricao = l.Descricao,
                TotalJogosLista = l.Itens.Count(i => i.EstaAtivo),
                CapasPreviewLista = l.Itens
                    .Where(i => i.EstaAtivo && i.Jogo != null && !string.IsNullOrEmpty(i.Jogo.Imagem))
                    .OrderBy(i => i.Ordem)
                    .Select(i => i.Jogo.Imagem)
                    .Take(4)
                    .ToList()
            }).ToList();

            // Combinar e ordenar todas as atividades por data decrescente
            return avaliacoes
                .Concat(zeradosItems)
                .Concat(listasItems)
                .OrderByDescending(item => item.DataAtividade)
                .Skip((pagina - 1) * itensPorPagina)
                .Take(itensPorPagina)
                .ToList();
        }
    }
}
