using System;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultDTO> AutenticarUsuario(UsuarioLoginDTO loginDTO, string? ipAddress = null);
        Task<UsuarioDTO> RegistrarUsuario(CriarUsuarioDTO usuarioDTO);
        Task<AuthResultDTO> RenovarTokenAsync(string refreshToken, string? ipAddress = null);
        Task<bool> RevogarTokenAsync(string refreshToken, string? ipAddress = null);
    }
}
