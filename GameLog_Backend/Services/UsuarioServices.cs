using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using GameLog_Backend.Configurations;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace GameLog_Backend.Services
{
    public class UsuarioServices
    {
        private readonly GameLogContext _context;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;


        public UsuarioServices(GameLogContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            _jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        }

        public async Task<(UsuarioDTO? usuario, string? token, DateTime expiraEm)> AutenticarUsuario(UsuarioLoginDTO loginDTO)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == loginDTO.Email);

            if (usuario == null || !VerificarSenha(loginDTO.Senha, usuario.Senha))
                return (null, null, DateTime.MinValue);

            var usuarioDTO = _mapper.Map<UsuarioDTO>(usuario);
            var token = GerarTokenJwt(usuario);
            var expiraEm = DateTime.Now.AddHours(_jwtSettings.ExpireHours);

            return (usuarioDTO, token, expiraEm);
        }

        private string GerarTokenJwt(Usuario usuario)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
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

        public IEnumerable<UsuarioDTO> ListarUsuarios()
        {
            var usuarios = _context.Usuarios
                .Select(u => _mapper.Map<UsuarioDTO>(u))
                .ToList();

            return usuarios;
        }

        public UsuarioDTO? ObterUsuarioPorId(int id)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Id == id);

            return usuario != null ? _mapper.Map<UsuarioDTO>(usuario) : null;
        }

        public async Task<UsuarioDTO> CriarUsuario(CriarUsuarioDTO usuarioDTO)
        {
            if (await EmailEmUso(usuarioDTO.Email))
            {
                throw new Exception("Email já está em uso");
            }

            if (await NomeUsuarioEmUso(usuarioDTO.NomeUsuario))
            {
                throw new Exception("Nome de usuário já está em uso");
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
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return Convert.ToBase64String(bytes);
        }

        private bool VerificarSenha(string senha, string senhaHash)
        {
            return HashSenha(senha) == senhaHash;
        }

        public async Task<bool> EmailEmUso(string email)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Email == email);
        }

        public async Task<bool> NomeUsuarioEmUso(string nomeUsuario)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.NomeUsuario == nomeUsuario);
        }

        public async Task<UsuarioDTO?> EditarUsuario(int id, string senhaAtual, EditarUsuarioDTO usuarioDTO)
        {
            var usuarioExistente = await _context.Usuarios.FindAsync(id);
            if (usuarioExistente == null || !VerificarSenha(senhaAtual, usuarioExistente.Senha))
            {
                return null;
            }

            if (usuarioDTO.Email != usuarioExistente.Email && await EmailEmUso(usuarioDTO.Email))
            {
                throw new Exception("O novo email já está em uso por outro usuário");
            }

            if (usuarioDTO.NomeUsuario != usuarioExistente.NomeUsuario && await NomeUsuarioEmUso(usuarioDTO.NomeUsuario))
            {
                throw new Exception("O novo nome de usuário já está em uso");
            }

            _mapper.Map(usuarioDTO, usuarioExistente);

            if (!string.IsNullOrEmpty(usuarioDTO.SenhaAtual))
            {
                usuarioExistente.Senha = HashSenha(usuarioDTO.NovaSenha);
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<UsuarioDTO>(usuarioExistente);
        }

        public async Task<bool> DeletarUsuario(int id, string senhaAtual)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null || !VerificarSenha(senhaAtual, usuario.Senha))
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
                .Where(a => a.Usuario.Id == id && a.EstaAtivo)
                .Include(a => a.Jogo)
                    .ThenInclude(j => j.Generos) 
                .ToListAsync(); 

            if (!avaliacoesDoUsuario.Any())
            {
                return new List<GeneroFavoritoDTO>();
            }

            var generosComNotas = avaliacoesDoUsuario
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
            var nomePrimeiroGeneroFavorito = topGeneros.FirstOrDefault()?.Genero; 

            if (!topGeneros.Any())
            {
                Console.WriteLine($"Nenhum gênero favorito identificado para o usuário {usuarioId}. Retornando lista vazia de recomendações.");
                return Enumerable.Empty<JogoRecomendacaoDTO>();
            }

            var jogosAvaliadosIds = await _context.Avaliacoes
                .Where(a => a.Usuario.Id == usuarioId && a.EstaAtivo)
                .Select(a => a.Jogo.Id)
                .ToListAsync();

            var generosParaBuscar = topGeneros.Select(g => g.Genero).ToList();

            var jogosRecomendados = await _context.Jogos
                .Where(j => j.Generos.Any(g => generosParaBuscar.Contains(g.TituloGenero)) &&
                            !jogosAvaliadosIds.Contains(j.Id) &&
                            j.EstaAtivo)
                .OrderByDescending(j => j.DataLancamento)
                .Take(10)
                .Select(j => new JogoRecomendacaoDTO
                {
                    JogoId = j.Id,
                    Titulo = j.Titulo,
                    Descricao = j.Descricao,
                    Imagem = j.Imagem,
                    DataLancamento = j.DataLancamento,
                    GeneroFavorito = nomePrimeiroGeneroFavorito
                })
                .ToListAsync();

            if (!jogosRecomendados.Any())
            {
                Console.WriteLine($"Nenhum jogo encontrado para recomendar nos top gêneros ({string.Join(", ", generosParaBuscar)}) para o usuário {usuarioId}.");
            }

            return jogosRecomendados;
        }
    }
}