using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using GameLog_Backend.Configurations;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GameLog_Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly GameLogContext _context;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;

        public AuthService(GameLogContext context, IMapper mapper, IOptions<JwtSettings> jwtOptions)
        {
            _context = context;
            _mapper = mapper;
            _jwtSettings = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        }

        public async Task<(UsuarioDTO? usuario, string? token, DateTime expiraEm)> AutenticarUsuario(UsuarioLoginDTO loginDTO)
        {
            if (string.IsNullOrWhiteSpace(loginDTO.Email) || string.IsNullOrWhiteSpace(loginDTO.Senha))
            {
                return (null, null, DateTime.MinValue);
            }

            var email = loginDTO.Email.Trim().ToLower();
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email && u.EstaAtivo);

            if (usuario == null)
                return (null, null, DateTime.MinValue);

            var (valida, precisaMigrar) = VerificarEMigrarSenha(loginDTO.Senha, usuario.Senha);
            if (!valida)
                return (null, null, DateTime.MinValue);

            // Migração transparente de hash legado SHA-256 para BCrypt
            if (precisaMigrar)
            {
                usuario.Senha = HashSenha(loginDTO.Senha);
                await _context.SaveChangesAsync();
            }

            var usuarioDTO = _mapper.Map<UsuarioDTO>(usuario);
            var token = GerarTokenJwt(usuario);
            var expiraEm = DateTime.UtcNow.AddHours(_jwtSettings.ExpireHours);

            return (usuarioDTO, token, expiraEm);
        }

        public async Task<UsuarioDTO> RegistrarUsuario(CriarUsuarioDTO usuarioDTO)
        {
            usuarioDTO.Email = usuarioDTO.Email?.Trim() ?? string.Empty;
            usuarioDTO.NomeUsuario = usuarioDTO.NomeUsuario?.Trim() ?? string.Empty;

            ValidarEmailESenha(usuarioDTO.Email, usuarioDTO.Senha, usuarioDTO.NomeUsuario);

            var emailLimpo = usuarioDTO.Email.ToLower();
            if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == emailLimpo && u.EstaAtivo))
            {
                throw new InvalidOperationException("Este e-mail já está em uso por outra conta.");
            }

            var nomeLimpo = usuarioDTO.NomeUsuario.ToLower();
            if (await _context.Usuarios.AnyAsync(u => u.NomeUsuario.ToLower() == nomeLimpo && u.EstaAtivo))
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

        public string GerarTokenJwt(Usuario usuario)
        {
            var keyString = !string.IsNullOrEmpty(_jwtSettings.Key) && _jwtSettings.Key != "{JWT_SECRET}"
                ? _jwtSettings.Key
                : throw new InvalidOperationException("Chave JWT não configurada.");

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

        public string HashSenha(string senha)
        {
            return BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 11);
        }

        public (bool Valida, bool PrecisaMigrar) VerificarEMigrarSenha(string senha, string senhaHash)
        {
            if (string.IsNullOrEmpty(senha) || string.IsNullOrEmpty(senhaHash))
                return (false, false);

            try
            {
                if (senhaHash.StartsWith(""))
                {
                    return (BCrypt.Net.BCrypt.Verify(senha, senhaHash), false);
                }
                
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(senha));
                var legacyHash = Convert.ToBase64String(bytes);
                var match = legacyHash == senhaHash;
                return (match, match);
            }
            catch
            {
                return (false, false);
            }
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
                if (trimmed.Length < 3 || trimmed.Length > 30)
                {
                    throw new ArgumentException("O nome de usuário deve ter entre 3 e 30 caracteres.");
                }

                if (!Regex.IsMatch(trimmed, @"^[a-zA-Z0-9_\.]+$"))
                {
                    throw new ArgumentException("O nome de usuário pode conter apenas letras, números, ponto (.) e sublinhado (_).");
                }
            }
        }
    }
}
