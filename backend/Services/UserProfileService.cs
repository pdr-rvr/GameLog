using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly GameLogContext _context;
        private readonly IMapper _mapper;
        private readonly IRecomendacaoService _recomendacaoService;

        public UserProfileService(
            GameLogContext context,
            IMapper mapper,
            IRecomendacaoService recomendacaoService)
        {
            _context = context;
            _mapper = mapper;
            _recomendacaoService = recomendacaoService;
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
            if (usuarioExistente == null || !usuarioExistente.EstaAtivo)
            {
                return null;
            }

            if (!VerificarSenha(senhaAtual, usuarioExistente.Senha))
            {
                return null;
            }

            usuarioDTO.Email = string.IsNullOrWhiteSpace(usuarioDTO.Email) ? usuarioExistente.Email : usuarioDTO.Email.Trim();
            usuarioDTO.NomeUsuario = string.IsNullOrWhiteSpace(usuarioDTO.NomeUsuario) ? usuarioExistente.NomeUsuario : usuarioDTO.NomeUsuario.Trim();
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
            return await _recomendacaoService.IdentificaTopNGenerosFavoritos(id, topN);
        }

        public async Task<IEnumerable<JogoRecomendacaoDTO>> RecomendarJogos(Guid usuarioId)
        {
            return await _recomendacaoService.RecomendarJogos(usuarioId);
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
                if (senhaHash.StartsWith(""))
                {
                    return BCrypt.Net.BCrypt.Verify(senha, senhaHash);
                }

                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(senha));
                var legacyHash = Convert.ToBase64String(bytes);
                return legacyHash == senhaHash;
            }
            catch
            {
                return false;
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
