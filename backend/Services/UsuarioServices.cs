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

        public async Task<UsuarioDTO?> ObterUsuarioPorId(Guid id)
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

        public async Task<UsuarioDTO?> EditarUsuario(Guid id, string senhaAtual, EditarUsuarioDTO usuarioDTO)
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

            if (!string.IsNullOrWhiteSpace(usuarioDTO.NovaSenha))
            {
                usuarioExistente.Senha = HashSenha(usuarioDTO.NovaSenha);
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<UsuarioDTO>(usuarioExistente);
        }

        public async Task<bool> DeletarUsuario(Guid id, string senhaAtual)
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

        public async Task<List<GeneroFavoritoDTO>> IdentificaTopNGenerosFavoritos(Guid id, int topN = 5)
        {
            var favoritos = await _context.JogosFavoritosUsuarios
                .AsNoTracking()
                .Where(f => f.Usuario.Id == id && f.EstaAtivo)
                .Include(f => f.Jogo)
                    .ThenInclude(j => j.Generos)
                .ToListAsync();

            var avaliacoes = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Usuario.Id == id && a.EstaAtivo)
                .Include(a => a.Jogo)
                    .ThenInclude(j => j.Generos)
                .ToListAsync();

            var biblioteca = await _context.ItensBiblioteca
                .AsNoTracking()
                .Where(b => b.Usuario.Id == id && b.EstaAtivo)
                .Include(b => b.Jogo)
                    .ThenInclude(j => j.Generos)
                .ToListAsync();

            var pontuacaoGeneros = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            foreach (var fav in favoritos.Where(f => f.Jogo?.Generos != null))
            {
                foreach (var g in fav.Jogo.Generos)
                {
                    pontuacaoGeneros[g.TituloGenero] = pontuacaoGeneros.GetValueOrDefault(g.TituloGenero) + 15.0;
                }
            }

            foreach (var av in avaliacoes.Where(a => a.Jogo?.Generos != null))
            {
                foreach (var g in av.Jogo.Generos)
                {
                    double peso = av.Nota >= 4 ? 2.5 : (av.Nota == 3 ? 1.0 : -1.0);
                    pontuacaoGeneros[g.TituloGenero] = pontuacaoGeneros.GetValueOrDefault(g.TituloGenero) + (av.Nota * peso);
                }
            }

            foreach (var bib in biblioteca.Where(b => b.Jogo?.Generos != null))
            {
                foreach (var g in bib.Jogo.Generos)
                {
                    pontuacaoGeneros[g.TituloGenero] = pontuacaoGeneros.GetValueOrDefault(g.TituloGenero) + 3.0;
                }
            }

            var topGeneros = pontuacaoGeneros
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .Take(topN)
                .Select(kv => new GeneroFavoritoDTO { Genero = kv.Key })
                .ToList();

            return topGeneros;
        }

        public async Task<IEnumerable<JogoRecomendacaoDTO>> RecomendarJogos(Guid usuarioId)
        {
            var jogosAvaliadosIds = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Usuario.Id == usuarioId && a.EstaAtivo)
                .Select(a => a.Jogo.Id)
                .ToListAsync();

            var jogosFavoritos = await _context.JogosFavoritosUsuarios
                .AsNoTracking()
                .Where(f => f.Usuario.Id == usuarioId && f.EstaAtivo)
                .Include(f => f.Jogo)
                    .ThenInclude(j => j.Generos)
                .Include(f => f.Jogo)
                    .ThenInclude(j => j.Empresa)
                .ToListAsync();

            var jogosFavoritosIds = jogosFavoritos.Select(f => f.Jogo.Id).ToList();

            var bibliotecaIds = await _context.ItensBiblioteca
                .AsNoTracking()
                .Where(b => b.Usuario.Id == usuarioId && b.EstaAtivo && b.Status != StatusJogo.QueroJogar)
                .Select(b => b.Jogo.Id)
                .ToListAsync();

            var jogosExcluidos = new HashSet<Guid>(jogosAvaliadosIds.Concat(jogosFavoritosIds).Concat(bibliotecaIds));

            var afinidadeGeneros = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var afinidadeEstudios = new Dictionary<Guid, double>();
            var nomesEstudiosFavoritos = new Dictionary<Guid, string>();

            foreach (var fav in jogosFavoritos.Where(f => f.Jogo != null))
            {
                if (fav.Jogo.Generos != null)
                {
                    foreach (var g in fav.Jogo.Generos)
                    {
                        afinidadeGeneros[g.TituloGenero] = afinidadeGeneros.GetValueOrDefault(g.TituloGenero) + 15.0;
                    }
                }
                if (fav.Jogo.Empresa != null)
                {
                    afinidadeEstudios[fav.Jogo.Empresa.Id] = afinidadeEstudios.GetValueOrDefault(fav.Jogo.Empresa.Id) + 12.0;
                    nomesEstudiosFavoritos[fav.Jogo.Empresa.Id] = fav.Jogo.Empresa.NomeEmpresa;
                }
            }

            var avaliacoesCompletas = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.Usuario.Id == usuarioId && a.EstaAtivo)
                .Include(a => a.Jogo)
                    .ThenInclude(j => j.Generos)
                .Include(a => a.Jogo)
                    .ThenInclude(j => j.Empresa)
                .ToListAsync();

            foreach (var av in avaliacoesCompletas.Where(a => a.Jogo != null))
            {
                double multiplicador = av.Nota >= 4 ? 2.5 : (av.Nota == 3 ? 1.0 : -1.5);
                double scoreAcao = av.Nota * multiplicador;

                if (av.Jogo.Generos != null)
                {
                    foreach (var g in av.Jogo.Generos)
                    {
                        afinidadeGeneros[g.TituloGenero] = afinidadeGeneros.GetValueOrDefault(g.TituloGenero) + scoreAcao;
                    }
                }
                if (av.Jogo.Empresa != null && av.Nota >= 4)
                {
                    afinidadeEstudios[av.Jogo.Empresa.Id] = afinidadeEstudios.GetValueOrDefault(av.Jogo.Empresa.Id) + scoreAcao;
                    nomesEstudiosFavoritos[av.Jogo.Empresa.Id] = av.Jogo.Empresa.NomeEmpresa;
                }
            }

            var bibliotecaCompleta = await _context.ItensBiblioteca
                .AsNoTracking()
                .Where(b => b.Usuario.Id == usuarioId && b.EstaAtivo)
                .Include(b => b.Jogo)
                    .ThenInclude(j => j.Generos)
                .Include(b => b.Jogo)
                    .ThenInclude(j => j.Empresa)
                .ToListAsync();

            foreach (var bib in bibliotecaCompleta.Where(b => b.Jogo != null))
            {
                if (bib.Jogo.Generos != null)
                {
                    foreach (var g in bib.Jogo.Generos)
                    {
                        afinidadeGeneros[g.TituloGenero] = afinidadeGeneros.GetValueOrDefault(g.TituloGenero) + 4.0;
                    }
                }
                if (bib.Jogo.Empresa != null)
                {
                    afinidadeEstudios[bib.Jogo.Empresa.Id] = afinidadeEstudios.GetValueOrDefault(bib.Jogo.Empresa.Id) + 3.0;
                    nomesEstudiosFavoritos[bib.Jogo.Empresa.Id] = bib.Jogo.Empresa.NomeEmpresa;
                }
            }

            var seguidosIds = await _context.SegueUsuarios
                .AsNoTracking()
                .Where(s => s.UsuarioSeguidor.Id == usuarioId && s.EstaAtivo)
                .Select(s => s.UsuarioSeguido.Id)
                .ToListAsync();

            var socialBoostJogos = new Dictionary<Guid, double>();
            if (seguidosIds.Any())
            {
                var avaliacoesSeguidos = await _context.Avaliacoes
                    .AsNoTracking()
                    .Where(a => seguidosIds.Contains(a.Usuario.Id) && a.EstaAtivo && a.Nota >= 4)
                    .Select(a => new { JogoId = a.Jogo.Id, Nota = a.Nota })
                    .ToListAsync();

                foreach (var av in avaliacoesSeguidos)
                {
                    socialBoostJogos[av.JogoId] = socialBoostJogos.GetValueOrDefault(av.JogoId) + (av.Nota * 1.5);
                }
            }

            var generosPositivos = afinidadeGeneros
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .Take(6)
                .ToList();

            if (!generosPositivos.Any() && !afinidadeEstudios.Any())
            {
                var jogosBase = await _context.Jogos
                    .AsNoTracking()
                    .Include(j => j.Generos)
                    .Include(j => j.Empresa)
                    .Where(j => j.EstaAtivo && !jogosExcluidos.Contains(j.Id))
                    .ToListAsync();

                var statsGeral = await _context.Avaliacoes
                    .AsNoTracking()
                    .Where(a => a.EstaAtivo)
                    .GroupBy(a => a.Jogo.Id)
                    .Select(g => new
                    {
                        JogoId = g.Key,
                        Media = g.Average(x => (double)x.Nota),
                        Total = g.Count()
                    })
                    .ToDictionaryAsync(x => x.JogoId);

                var topObrasPrimas = jogosBase
                    .Select(j =>
                    {
                        statsGeral.TryGetValue(j.Id, out var s);
                        return new
                        {
                            Jogo = j,
                            Media = s != null ? (double?)s.Media : null,
                            TotalReviews = s?.Total ?? 0
                        };
                    })
                    .OrderByDescending(x => x.TotalReviews > 0 ? (x.Media ?? 0) : 0)
                    .ThenByDescending(x => x.TotalReviews)
                    .ThenByDescending(x => x.Jogo.DataLancamento)
                    .Take(12)
                    .ToList();

                return topObrasPrimas.Select(item => new JogoRecomendacaoDTO
                {
                    JogoId = item.Jogo.Id,
                    Titulo = item.Jogo.Titulo,
                    Descricao = item.Jogo.Descricao,
                    Imagem = item.Jogo.Imagem,
                    DataLancamento = item.Jogo.DataLancamento,
                    ClassificacaoIndicativa = item.Jogo.ClassificacaoIndicativa,
                    GeneroFavorito = item.Jogo.Generos.FirstOrDefault()?.TituloGenero ?? "Destaque",
                    NomeEmpresa = item.Jogo.Empresa?.NomeEmpresa,
                    MediaAvaliacoes = item.Media.HasValue ? Math.Round(item.Media.Value, 1) : null,
                    MotivoRecomendacao = item.TotalReviews > 0 ? "Aclamado pela Comunidade" : "Destaque do Catálogo",
                    Score = (item.Media ?? 4.0) * 10
                });
            }

            var afinidadeEstudiosIds = afinidadeEstudios.Keys.ToList();

            var candidatos = await _context.Jogos
                .AsNoTracking()
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Where(j => j.EstaAtivo && !jogosExcluidos.Contains(j.Id) &&
                           (j.Generos.Any(g => generosPositivos.Contains(g.TituloGenero)) ||
                            (j.Empresa != null && afinidadeEstudiosIds.Contains(j.Empresa.Id))))
                .ToListAsync();

            var candidatosIds = candidatos.Select(c => c.Id).ToList();
            var statsCandidatos = await _context.Avaliacoes
                .AsNoTracking()
                .Where(a => a.EstaAtivo && candidatosIds.Contains(a.Jogo.Id))
                .GroupBy(a => a.Jogo.Id)
                .Select(g => new
                {
                    JogoId = g.Key,
                    Media = g.Average(x => (double)x.Nota)
                })
                .ToDictionaryAsync(x => x.JogoId, x => x.Media);

            var pontuados = new List<JogoRecomendacaoDTO>();

            foreach (var j in candidatos)
            {
                double scoreFinal = 0.0;
                string motivo = string.Empty;
                string generoDestaque = generosPositivos.FirstOrDefault() ?? "Recomendado";

                double scoreGeneroJogo = 0.0;
                int generosCombinados = 0;
                string? melhorGeneroMatch = null;

                foreach (var g in j.Generos)
                {
                    if (afinidadeGeneros.TryGetValue(g.TituloGenero, out double pesoG))
                    {
                        scoreGeneroJogo += pesoG;
                        generosCombinados++;
                        if (melhorGeneroMatch == null || pesoG > afinidadeGeneros.GetValueOrDefault(melhorGeneroMatch))
                        {
                            melhorGeneroMatch = g.TituloGenero;
                        }
                    }
                }

                if (generosCombinados > 1)
                {
                    scoreGeneroJogo *= 1.25;
                }

                scoreFinal += scoreGeneroJogo;
                if (melhorGeneroMatch != null)
                {
                    generoDestaque = melhorGeneroMatch;
                    motivo = $"Baseado no seu gosto por {melhorGeneroMatch}";
                }

                if (j.Empresa != null && afinidadeEstudios.TryGetValue(j.Empresa.Id, out double pesoEstudio))
                {
                    scoreFinal += pesoEstudio * 1.4;
                    motivo = $"Do estúdio {j.Empresa.NomeEmpresa}";
                }

                statsCandidatos.TryGetValue(j.Id, out double mediaComunidade);
                if (mediaComunidade > 0)
                {
                    scoreFinal += (mediaComunidade * 3.0);
                }

                if (socialBoostJogos.TryGetValue(j.Id, out double socialBonus))
                {
                    scoreFinal += socialBonus;
                    if (string.IsNullOrEmpty(motivo))
                    {
                        motivo = "Popular entre quem você segue";
                    }
                }

                int anosDesdeLancamento = Math.Max(0, DateTime.UtcNow.Year - j.DataLancamento.Year);
                if (anosDesdeLancamento <= 2)
                {
                    scoreFinal += 6.0;
                }
                else if (anosDesdeLancamento <= 5)
                {
                    scoreFinal += 3.0;
                }

                pontuados.Add(new JogoRecomendacaoDTO
                {
                    JogoId = j.Id,
                    Titulo = j.Titulo,
                    Descricao = j.Descricao,
                    Imagem = j.Imagem,
                    DataLancamento = j.DataLancamento,
                    ClassificacaoIndicativa = j.ClassificacaoIndicativa,
                    GeneroFavorito = generoDestaque,
                    NomeEmpresa = j.Empresa?.NomeEmpresa,
                    MediaAvaliacoes = mediaComunidade > 0 ? Math.Round(mediaComunidade, 1) : null,
                    MotivoRecomendacao = string.IsNullOrEmpty(motivo) ? "Recomendado para o seu perfil" : motivo,
                    Score = scoreFinal
                });
            }

            var ordenadosPorScore = pontuados
                .OrderByDescending(p => p.Score)
                .ThenByDescending(p => p.MediaAvaliacoes ?? 0)
                .ToList();

            var selecionados = new List<JogoRecomendacaoDTO>();
            var contagemPorEstudio = new Dictionary<string, int>();

            foreach (var item in ordenadosPorScore)
            {
                var est = item.NomeEmpresa ?? "Outro";
                int qtd = contagemPorEstudio.GetValueOrDefault(est);
                if (qtd < 2)
                {
                    selecionados.Add(item);
                    contagemPorEstudio[est] = qtd + 1;
                    if (selecionados.Count >= 12) break;
                }
            }

            if (selecionados.Count < 12)
            {
                var restantes = ordenadosPorScore
                    .Where(p => !selecionados.Any(s => s.JogoId == p.JogoId))
                    .Take(12 - selecionados.Count);
                selecionados.AddRange(restantes);
            }

            return selecionados;
        }

        // ======================= SISTEMA SOCIAL (SEGUIR & FEED) ======================= //

        public async Task<(bool Seguido, int TotalSeguidores)> AlternarSeguirUsuario(Guid seguidorId, Guid seguidoId)
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

        public async Task<bool> VerificarSeSegue(Guid seguidorId, Guid seguidoId)
        {
            if (seguidorId == seguidoId) return false;

            return await _context.SegueUsuarios
                .AnyAsync(s => s.UsuarioSeguidor.Id == seguidorId && s.UsuarioSeguido.Id == seguidoId && s.EstaAtivo);
        }

        public async Task<EstatisticasSociaisDTO> ObterEstatisticasSociais(Guid usuarioId, Guid? solicitanteId = null)
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

        public async Task<List<UsuarioConexaoDTO>> ObterSeguidores(Guid usuarioId, Guid? solicitanteId = null)
        {
            var conexoes = await _context.SegueUsuarios
                .AsNoTracking()
                .Include(s => s.UsuarioSeguidor)
                .Where(s => s.UsuarioSeguido.Id == usuarioId && s.EstaAtivo && s.UsuarioSeguidor.EstaAtivo)
                .ToListAsync();

            var seguidorIds = conexoes.Select(c => c.UsuarioSeguidor.Id).Distinct().ToList();

            var seguidosPeloSolicitante = new HashSet<Guid>();
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

        public async Task<List<UsuarioConexaoDTO>> ObterSeguindo(Guid usuarioId, Guid? solicitanteId = null)
        {
            var conexoes = await _context.SegueUsuarios
                .AsNoTracking()
                .Include(s => s.UsuarioSeguido)
                .Where(s => s.UsuarioSeguidor.Id == usuarioId && s.EstaAtivo && s.UsuarioSeguido.EstaAtivo)
                .ToListAsync();

            var seguidoIds = conexoes.Select(c => c.UsuarioSeguido.Id).Distinct().ToList();

            var seguidosPeloSolicitante = new HashSet<Guid>();
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

        public async Task<List<ItemFeedSocialDTO>> ObterFeedSocial(Guid usuarioId, int pagina = 1, int itensPorPagina = 20)
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
