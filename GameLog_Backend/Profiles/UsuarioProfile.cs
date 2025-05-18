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

            CreateMap<EditarUsuarioDTO, Usuario>()
                .ForMember(dest => dest.Senha, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}