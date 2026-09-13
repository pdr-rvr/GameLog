using System;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(UsuarioDTO? usuario, string? token, string? refreshToken, DateTime expiraEm)> AutenticarUsuario(UsuarioLoginDTO loginDTO, string? ipAddress = null);
        Task<UsuarioDTO> RegistrarUsuario(CriarUsuarioDTO usuarioDTO);
        Task<(UsuarioDTO usuario, string token, string novoRefreshToken, DateTime expiraEm)> RenovarTokenAsync(string refreshToken, string? ipAddress = null);
        Task<bool> RevogarTokenAsync(string refreshToken, string? ipAddress = null);
    }
}
