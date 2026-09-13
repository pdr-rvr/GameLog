using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Helpers;
using GameLog_Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Services
{
    public class BuscaGlobalService : IBuscaGlobalService
    {
        private readonly GameLogContext _context;
        private readonly IRawgApiService _rawgService;

        public BuscaGlobalService(GameLogContext context, IRawgApiService rawgService)
        {
            _context = context;
            _rawgService = rawgService;
        }

        public async Task<BuscaGlobalDTO> BuscarAsync(string? q, int limite = 5)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            {
                return new BuscaGlobalDTO { Termo = q?.Trim() ?? string.Empty };
            }

            var termo = q.Trim().ToLower();
            limite = Math.Clamp(limite, 1, 20);

            // 1. Busca em Jogos Locais com filtragem de relevância
            var jogosLocaisCandidatos = await _context.Jogos
                .AsNoTracking()
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Include(j => j.Publicadora)
                .Where(j => j.EstaAtivo && (
                    j.Titulo.ToLower().Contains(termo) ||
                    (j.Empresa != null && j.Empresa.NomeEmpresa.ToLower().Contains(termo)) ||
                    (j.Publicadora != null && j.Publicadora.NomeEmpresa.ToLower().Contains(termo)) ||
                    j.Generos.Any(g => g.TituloGenero.ToLower().Contains(termo))
                ))
                .Take(limite * 3)
                .ToListAsync();

            var jogos = jogosLocaisCandidatos
                .Where(j => RelevanciaBuscaHelper.CorrespondeBusca(j.Titulo, j.Empresa?.NomeEmpresa, j.Publicadora?.NomeEmpresa, termo))
                .OrderByDescending(j => RelevanciaBuscaHelper.CalcularScoreRelevancia(j.Titulo, termo))
                .Take(limite)
                .Select(j => new BuscaItemJogoDTO
                {
                    JogoId = j.Id,
                    Titulo = j.Titulo,
                    Imagem = j.Imagem,
                    AnoLancamento = j.DataLancamento.Year,
                    NomeEmpresa = j.Empresa != null ? j.Empresa.NomeEmpresa : null,
                    Generos = j.Generos.Select(g => g.TituloGenero).ToList(),
                    MediaAvaliacoes = _context.Avaliacoes
                        .Where(a => a.Jogo.Id == j.Id && a.EstaAtivo)
                        .Average(a => (double?)a.Nota),
                    EhExterno = false,
                    RawgId = null
                })
                .ToList();

            // Complementar com RAWG se houver poucos resultados locais
            if (jogos.Count < limite)
            {
                try
                {
                    var rawgRes = await _rawgService.BuscarJogosExternos(termo, 1, limite - jogos.Count + 2);
                    if (rawgRes?.Jogos != null)
                    {
                        var titulosLocais = new HashSet<string>(jogos.Select(j => j.Titulo.ToLower()));

                        foreach (var rg in rawgRes.Jogos)
                        {
                            if (jogos.Count >= limite) break;
                            if (titulosLocais.Contains(rg.Titulo.ToLower())) continue;

                            jogos.Add(new BuscaItemJogoDTO
                            {
                                JogoId = rg.LocalJogoId ?? Guid.Empty,
                                Titulo = rg.Titulo,
                                Imagem = rg.Imagem,
                                AnoLancamento = rg.AnoLancamento,
                                NomeEmpresa = rg.NomeEmpresa,
                                Generos = rg.Generos,
                                MediaAvaliacoes = null,
                                RawgId = rg.RawgId,
                                EhExterno = !rg.JaImportado
                            });

                            titulosLocais.Add(rg.Titulo.ToLower());
                        }
                    }
                }
                catch
                {
                    // Falha de rede da RAWG não quebra a busca local
                }
            }

            // 2. Busca em Usuários
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.EstaAtivo && u.NomeUsuario.ToLower().Contains(termo))
                .Take(limite)
                .Select(u => new BuscaItemUsuarioDTO
                {
                    UsuarioId = u.Id,
                    NomeUsuario = u.NomeUsuario,
                    FotoDePerfil = u.FotoDePerfil,
                    Bio = u.Bio
                })
                .ToListAsync();

            // 3. Busca em Listas Públicas
            var listas = await _context.ListasDeJogos
                .AsNoTracking()
                .Include(l => l.Usuario)
                .Include(l => l.Itens)
                    .ThenInclude(i => i.Jogo)
                .Where(l => l.EstaAtivo && l.EstaPublica && (
                    l.Titulo.ToLower().Contains(termo) ||
                    (l.Descricao != null && l.Descricao.ToLower().Contains(termo))
                ))
                .Take(limite)
                .Select(l => new BuscaItemListaDTO
                {
                    ListaId = l.Id,
                    Titulo = l.Titulo,
                    Descricao = l.Descricao,
                    UsuarioId = l.Usuario.Id,
                    NomeCriador = l.Usuario.NomeUsuario,
                    TotalJogos = l.Itens.Count(i => i.EstaAtivo),
                    CapasPreview = l.Itens
                        .Where(i => i.EstaAtivo && i.Jogo != null && !string.IsNullOrEmpty(i.Jogo.Imagem))
                        .OrderBy(i => i.Ordem)
                        .Select(i => i.Jogo.Imagem)
                        .Take(4)
                        .ToList()
                })
                .ToListAsync();

            return new BuscaGlobalDTO
            {
                Termo = q.Trim(),
                Jogos = jogos,
                Usuarios = usuarios,
                Listas = listas
            };
        }
    }
}
