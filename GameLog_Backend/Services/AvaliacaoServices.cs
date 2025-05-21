using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Services
{
    public class AvaliacaoServices
    {
        private readonly GameLogContext _context;
        private readonly IMapper _mapper;

        public AvaliacaoServices(GameLogContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<Jogo> VerificarJogoExiste(int jogoId)
        {
            var jogo = await _context.Jogos.FindAsync(jogoId);
            if (jogo == null)
                throw new Exception("Jogo não encontrado");
            return jogo;
        }

        private async Task VerificarAvaliacaoDuplicada(int usuarioId, int jogoId)
        {
            var usuarioComAvaliacoes = await _context.Usuarios
                .Include(u => u.Avaliacoes)
                .ThenInclude(a => a.Jogo)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuarioComAvaliacoes == null)
                throw new Exception("Usuário não encontrado");

            if (usuarioComAvaliacoes.Avaliacoes.Any(a =>
                a.Jogo != null && a.Jogo.Id == jogoId && a.EstaAtivo))
            {
                throw new Exception("Você já possui uma avaliação ativa para este jogo");
            }
        }

        public async Task<AvaliacaoDTO> CriarAvaliacao(CriarAvaliacaoDTO avaliacaoDTO, int usuarioId)
        {
            await VerificarJogoExiste(avaliacaoDTO.JogoId);
            await VerificarAvaliacaoDuplicada(usuarioId, avaliacaoDTO.JogoId);

            var avaliacao = _mapper.Map<Avaliacao>(avaliacaoDTO);
            avaliacao.DataPublicacao = DateTime.UtcNow;
            avaliacao.EstaAtivo = true;
            avaliacao.Jogo = await _context.Jogos.FindAsync(avaliacaoDTO.JogoId);

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            usuario?.Avaliacoes.Add(avaliacao);

            await _context.SaveChangesAsync();
            return _mapper.Map<AvaliacaoDTO>(avaliacao);
        }

        public async Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoes()
        {
            return await _context.Avaliacoes
                .Where(a => a.EstaAtivo)
                .OrderByDescending(a => a.DataPublicacao)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    JogoId = a.Jogo.Id,
                    NomeJogo = a.Jogo.Titulo,
                    NomeUsuario = _context.Usuarios
                        .Where(u => u.Avaliacoes.Any(av => av.Id == a.Id))
                        .Select(u => u.NomeUsuario)
                        .FirstOrDefault(),
                    TextoAvaliacao = a.TextoAvaliacao,
                    DataPublicacao = a.DataPublicacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count
                })
                .ToListAsync();
        }

        public async Task<AvaliacaoDTO?> ObterAvaliacaoPorId(int id)
        {
            return await _context.Avaliacoes
                .Where(a => a.Id == id && a.EstaAtivo)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    JogoId = a.Jogo.Id,
                    NomeJogo = a.Jogo.Titulo,
                    NomeUsuario = _context.Usuarios
                        .Where(u => u.Avaliacoes.Any(av => av.Id == a.Id))
                        .Select(u => u.NomeUsuario)
                        .FirstOrDefault(),
                    TextoAvaliacao = a.TextoAvaliacao,
                    DataPublicacao = a.DataPublicacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count
                })
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<AvaliacaoDTO>> ListarAvaliacoesPorUsuario(int usuarioId)
        {
            var NomeUsuario = _context.Usuarios
                .Where(u => u.Id == usuarioId)
                .Select(u => u.NomeUsuario)
                .FirstOrDefault();

            var avaliacoes = await _context.Usuarios
                .Where(u => u.Id == usuarioId)
                .SelectMany(u => u.Avaliacoes)
                .Where(a => a.EstaAtivo)
                .OrderByDescending(a => a.DataPublicacao)
                .Include(a => a.Jogo)
                .Include(a => a.CurtidasDeAvaliacao)
                .Select(a => new AvaliacaoDTO
                {
                    AvaliacaoId = a.Id,
                    Nota = a.Nota,
                    JogoId = a.Jogo.Id,
                    NomeJogo = a.Jogo.Titulo,  
                    NomeUsuario = NomeUsuario,  
                    TextoAvaliacao = a.TextoAvaliacao,
                    DataPublicacao = a.DataPublicacao,
                    TotalCurtidas = a.CurtidasDeAvaliacao.Count
                })
                .ToListAsync();

            return avaliacoes;
        }

        public async Task<AvaliacaoDTO?> EditarAvaliacao(int id, EditarAvaliacaoDTO avaliacaoDTO, int usuarioId)
        {
            var avaliacao = await _context.Usuarios
                .Where(u => u.Id == usuarioId)
                .SelectMany(u => u.Avaliacoes)
                .FirstOrDefaultAsync(a => a.Id == id && a.EstaAtivo);

            if (avaliacao == null)
                return null;

            _mapper.Map(avaliacaoDTO, avaliacao);
            await _context.SaveChangesAsync();

            return _mapper.Map<AvaliacaoDTO>(avaliacao);
        }

        public async Task<bool> DeletarAvaliacao(int id, int usuarioId)
        {
            var avaliacao = await _context.Usuarios
                .Where(u => u.Id == usuarioId)
                .SelectMany(u => u.Avaliacoes)
                .FirstOrDefaultAsync(a => a.Id == id && a.EstaAtivo);

            if (avaliacao == null)
                return false;

            avaliacao.EstaAtivo = false; 
            await _context.SaveChangesAsync();

            return true;
        }
    }
}