using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Services
{
    public class SocialService : ISocialService
    {
        private readonly GameLogContext _context;

        public SocialService(GameLogContext context)
        {
            _context = context;
        }

        public async Task<(bool Seguido, int TotalSeguidores)> AlternarSeguirUsuario(Guid seguidorId, Guid seguidoId)
        {
            if (seguidorId == seguidoId)
            {
                throw new InvalidOperationException("Você não pode seguir seu próprio perfil.");
            }

            var seguido = await _context.Usuarios.FindAsync(seguidoId);
            if (seguido == null || !seguido.EstaAtivo)
            {
                throw new KeyNotFoundException("Usuário não encontrado.");
            }

            var seguidor = await _context.Usuarios.FindAsync(seguidorId);
            if (seguidor == null || !seguidor.EstaAtivo)
            {
                throw new KeyNotFoundException("Usuário autenticado não encontrado.");
            }

            var relacaoExistente = await _context.SegueUsuarios
                .Include(s => s.UsuarioSeguidor)
                .Include(s => s.UsuarioSeguido)
                .FirstOrDefaultAsync(s => s.UsuarioSeguidor.Id == seguidorId && s.UsuarioSeguido.Id == seguidoId);

            bool novoEstado;
            if (relacaoExistente == null)
            {
                _context.SegueUsuarios.Add(new SegueUsuario
                {
                    UsuarioSeguidor = seguidor,
                    UsuarioSeguido = seguido,
                    EstaAtivo = true
                });
                novoEstado = true;
            }
            else
            {
                novoEstado = !relacaoExistente.EstaAtivo;
                relacaoExistente.EstaAtivo = novoEstado;
            }

            await _context.SaveChangesAsync();

            var totalSeguidores = await _context.SegueUsuarios
                .CountAsync(s => s.UsuarioSeguido.Id == seguidoId && s.EstaAtivo);

            return (novoEstado, totalSeguidores);
        }

        public async Task<bool> VerificarSeSegue(Guid seguidorId, Guid seguidoId)
        {
            if (seguidorId == seguidoId) return false;

            return await _context.SegueUsuarios
                .AnyAsync(s => s.UsuarioSeguidor.Id == seguidorId && s.UsuarioSeguido.Id == seguidoId && s.EstaAtivo);
        }

        public async Task<EstatisticasSociaisDTO> ObterEstatisticasSociais(Guid usuarioId, Guid? solicitanteId = null)
        {
            var totalSeguidores = await _context.SegueUsuarios
                .CountAsync(s => s.UsuarioSeguido.Id == usuarioId && s.EstaAtivo);

            var totalSeguindo = await _context.SegueUsuarios
                .CountAsync(s => s.UsuarioSeguidor.Id == usuarioId && s.EstaAtivo);

            var seguidoPorMim = solicitanteId.HasValue && await VerificarSeSegue(solicitanteId.Value, usuarioId);

            return new EstatisticasSociaisDTO
            {
                TotalSeguidores = totalSeguidores,
                TotalSeguindo = totalSeguindo,
                SeguidoPorMim = seguidoPorMim
            };
        }

        public async Task<List<UsuarioConexaoDTO>> ObterSeguidores(Guid usuarioId, Guid? solicitanteId = null)
        {
            var conexoes = await _context.SegueUsuarios
                .AsNoTracking()
                .Include(s => s.UsuarioSeguidor)
                .Where(s => s.UsuarioSeguido.Id == usuarioId && s.EstaAtivo && s.UsuarioSeguidor.EstaAtivo)
                .ToListAsync();

            var seguidorIds = conexoes.Select(c => c.UsuarioSeguidor.Id).Distinct().ToList();

            var seguidosPeloSolicitante = new HashSet<Guid>();
            if (solicitanteId.HasValue)
            {
                seguidosPeloSolicitante = (await _context.SegueUsuarios
                    .AsNoTracking()
                    .Where(s => s.UsuarioSeguidor.Id == solicitanteId.Value && seguidorIds.Contains(s.UsuarioSeguido.Id) && s.EstaAtivo)
                    .Select(s => s.UsuarioSeguido.Id)
                    .ToListAsync())
                    .ToHashSet();
            }

            return conexoes.Select(c => new UsuarioConexaoDTO
            {
                UsuarioId = c.UsuarioSeguidor.Id,
                NomeUsuario = c.UsuarioSeguidor.NomeUsuario,
                FotoPerfil = c.UsuarioSeguidor.FotoDePerfil,
                Bio = c.UsuarioSeguidor.Bio,
                SeguidoPorMim = solicitanteId.HasValue && seguidosPeloSolicitante.Contains(c.UsuarioSeguidor.Id)
            }).ToList();
        }

        public async Task<List<UsuarioConexaoDTO>> ObterSeguindo(Guid usuarioId, Guid? solicitanteId = null)
        {
            var conexoes = await _context.SegueUsuarios
                .AsNoTracking()
                .Include(s => s.UsuarioSeguido)
                .Where(s => s.UsuarioSeguidor.Id == usuarioId && s.EstaAtivo && s.UsuarioSeguido.EstaAtivo)
                .ToListAsync();

            var seguidoIds = conexoes.Select(c => c.UsuarioSeguido.Id).Distinct().ToList();

            var seguidosPeloSolicitante = new HashSet<Guid>();
            if (solicitanteId.HasValue)
            {
                seguidosPeloSolicitante = (await _context.SegueUsuarios
                    .AsNoTracking()
                    .Where(s => s.UsuarioSeguidor.Id == solicitanteId.Value && seguidoIds.Contains(s.UsuarioSeguido.Id) && s.EstaAtivo)
                    .Select(s => s.UsuarioSeguido.Id)
                    .ToListAsync())
                    .ToHashSet();
            }

            return conexoes.Select(c => new UsuarioConexaoDTO
            {
                UsuarioId = c.UsuarioSeguido.Id,
                NomeUsuario = c.UsuarioSeguido.NomeUsuario,
                FotoPerfil = c.UsuarioSeguido.FotoDePerfil,
                Bio = c.UsuarioSeguido.Bio,
                SeguidoPorMim = solicitanteId.HasValue && seguidosPeloSolicitante.Contains(c.UsuarioSeguido.Id)
            }).ToList();
        }
    }
}
