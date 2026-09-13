using System;
using System.Threading.Tasks;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(UsuarioDTO? usuario, string? token, DateTime expiraEm)> AutenticarUsuario(UsuarioLoginDTO loginDTO);
        Task<UsuarioDTO> RegistrarUsuario(CriarUsuarioDTO usuarioDTO);
    }
}
