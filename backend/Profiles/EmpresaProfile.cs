using AutoMapper;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;

namespace GameLog_Backend.Profiles
{
    public class EmpresaProfile : Profile
    {
        public EmpresaProfile()
        {
            CreateMap<Empresa, EmpresaDTO>()
                .ForMember(dest => dest.EmpresaId, opt => opt.MapFrom(src => src.Id));
        }
    }
}
