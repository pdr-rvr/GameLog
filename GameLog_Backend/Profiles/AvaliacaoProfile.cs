using AutoMapper;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;

namespace GameLog_Backend.Profiles
{
    public class AvaliacaoProfile : Profile
    {
        public AvaliacaoProfile()
        {
            CreateMap<Avaliacao, AvaliacaoDTO>()
                .ForMember(dest => dest.TotalCurtidas,
                           opt => opt.MapFrom(src => src.CurtidasDeAvaliacao.Count));

            CreateMap<CriarAvaliacaoDTO, Avaliacao>()
                .ForMember(dest => dest.DataPublicacao,
                           opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.EstaAtivo,
                           opt => opt.MapFrom(_ => true));

            CreateMap<EditarAvaliacaoDTO, Avaliacao>()
                .ForAllMembers(opts =>
                    opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}