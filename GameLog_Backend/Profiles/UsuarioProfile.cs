using AutoMapper;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;

namespace GameLog_Backend.Profiles
{
    public class UsuarioProfile : Profile
    {
        public UsuarioProfile()
        {
            CreateMap<Usuario, UsuarioDTO>();
            CreateMap<CriarUsuarioDTO, Usuario>()
                .ForMember(dest => dest.Senha, opt => opt.Ignore()); 
        }
    }
}