using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Services.Interfaces;

namespace GameLog_Backend.Services
{
    public class UsuarioServices : IUsuarioService
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IAuthService _authService;
        private readonly ISocialService _socialService;
        private readonly IFeedService _feedService;

        [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
        public UsuarioServices(
            IUserProfileService userProfileService,
            IAuthService authService,
            ISocialService socialService,
            IFeedService feedService)
        {
            _userProfileService = userProfileService;
            _authService = authService;
            _socialService = socialService;
            _feedService = feedService;
        }

        public UsuarioServices(
            GameLogContext context,
            IMapper mapper,
            Microsoft.Extensions.Options.IOptions<GameLog_Backend.Configurations.JwtSettings> jwtOptions)
            : this(
                new UserProfileService(context, mapper, new RecomendacaoService(context, new MemoryCacheService())),
                new AuthService(context, mapper, jwtOptions),
                new SocialService(context),
                new FeedService(context))
        {
        }

        public async Task<(UsuarioDTO? usuario, string? token, DateTime expiraEm)> AutenticarUsuario(UsuarioLoginDTO loginDTO)
        {
            var (usuario, token, _, expiraEm) = await _authService.AutenticarUsuario(loginDTO);
            return (usuario, token, expiraEm);
        }

        public async Task<IEnumerable<UsuarioDTO>> ListarUsuarios()
        {
            return await _userProfileService.ListarUsuarios();
        }

        public async Task<UsuarioDTO?> ObterUsuarioPorId(Guid id)
        {
            return await _userProfileService.ObterUsuarioPorId(id);
        }

        public async Task<UsuarioDTO> CriarUsuario(CriarUsuarioDTO usuarioDTO)
        {
            return await _authService.RegistrarUsuario(usuarioDTO);
        }

        public async Task<bool> EmailEmUso(string email)
        {
            return await _userProfileService.EmailEmUso(email);
        }

        public async Task<bool> NomeUsuarioEmUso(string nomeUsuario)
        {
            return await _userProfileService.NomeUsuarioEmUso(nomeUsuario);
        }

        public async Task<UsuarioDTO?> EditarUsuario(Guid id, string senhaAtual, EditarUsuarioDTO usuarioDTO)
        {
            return await _userProfileService.EditarUsuario(id, senhaAtual, usuarioDTO);
        }

        public async Task<bool> DeletarUsuario(Guid id, string senhaAtual)
        {
            return await _userProfileService.DeletarUsuario(id, senhaAtual);
        }

        public async Task<List<GeneroFavoritoDTO>> IdentificaTopNGenerosFavoritos(Guid id, int topN = 5)
        {
            return await _userProfileService.IdentificaTopNGenerosFavoritos(id, topN);
        }

        public async Task<IEnumerable<JogoRecomendacaoDTO>> RecomendarJogos(Guid usuarioId)
        {
            return await _userProfileService.RecomendarJogos(usuarioId);
        }

        public async Task<(bool Seguido, int TotalSeguidores)> AlternarSeguirUsuario(Guid seguidorId, Guid seguidoId)
        {
            return await _socialService.AlternarSeguirUsuario(seguidorId, seguidoId);
        }

        public async Task<bool> VerificarSeSegue(Guid seguidorId, Guid seguidoId)
        {
            return await _socialService.VerificarSeSegue(seguidorId, seguidoId);
        }

        public async Task<EstatisticasSociaisDTO> ObterEstatisticasSociais(Guid usuarioId, Guid? solicitanteId = null)
        {
            return await _socialService.ObterEstatisticasSociais(usuarioId, solicitanteId);
        }

        public async Task<List<UsuarioConexaoDTO>> ObterSeguidores(Guid usuarioId, Guid? solicitanteId = null)
        {
            return await _socialService.ObterSeguidores(usuarioId, solicitanteId);
        }

        public async Task<List<UsuarioConexaoDTO>> ObterSeguindo(Guid usuarioId, Guid? solicitanteId = null)
        {
            return await _socialService.ObterSeguindo(usuarioId, solicitanteId);
        }

        public async Task<List<ItemFeedSocialDTO>> ObterFeedSocial(Guid usuarioId, int pagina = 1, int itensPorPagina = 20)
        {
            return await _feedService.ObterFeedSocial(usuarioId, pagina, itensPorPagina);
        }

        public async Task<List<ItemAtividadeTimelineDTO>> ObterTimelineAtividades(Guid usuarioId, int pagina = 1, int itensPorPagina = 30)
        {
            return await _feedService.ObterTimelineAtividades(usuarioId, pagina, itensPorPagina);
        }
    }
}
