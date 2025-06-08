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

        public async Task<GeneroFavoritoDTO?> IdentificaGeneroFavorito(int id)
        {
            var generoFavorito = await _context.Avaliacoes
                .Where(a => a.Usuario.Id == id && a.EstaAtivo)
                .SelectMany(a => a.Jogo.Generos)
                .GroupBy(g => g.TituloGenero)
                .Select(g => new
                {
                    Genero = g.Key,
                    MediaNotas = _context.Avaliacoes
                        .Where(a => a.Usuario.Id == id &&
                                   a.EstaAtivo &&
                                   a.Jogo.Generos.Any(gen => gen.TituloGenero == g.Key))
                        .Average(a => a.Nota),
                    QuantidadeJogos = g.Count()
                })
                .OrderByDescending(x => x.MediaNotas)
                .ThenByDescending(x => x.QuantidadeJogos)
                .FirstOrDefaultAsync();

            if (generoFavorito == null)
                return null;

            return new GeneroFavoritoDTO
            {
                Genero = generoFavorito.Genero
            };
        }

        public async Task<IEnumerable<JogoRecomendacaoDTO>> RecomendarJogos(int usuarioId)
        {
            var generoFavorito = await IdentificaGeneroFavorito(usuarioId);

            if (generoFavorito == null)
            {
                return Enumerable.Empty<JogoRecomendacaoDTO>();
            }

            var jogosAvaliados = await _context.Avaliacoes
                .Where(a => a.Usuario.Id == usuarioId && a.EstaAtivo)
                .Select(a => a.Jogo.Id)
                .ToListAsync();

            var jogosRecomendados = await _context.Jogos
                .Where(j => j.Generos.Any(g => g.TituloGenero == generoFavorito.Genero) &&
                            !jogosAvaliados.Contains(j.Id) &&
                            j.EstaAtivo)
                .OrderByDescending(j => j.DataLancamento) 
                .Take(3) 
                .Select(j => new JogoRecomendacaoDTO
                {
                    JogoId = j.Id,
                    Titulo = j.Titulo,
                    Descricao = j.Descricao,
                    Imagem = j.Imagem,
                    DataLancamento = j.DataLancamento,
                    GeneroFavorito = generoFavorito.Genero
                })
                .ToListAsync();

            return jogosRecomendados;
        }
    }
}