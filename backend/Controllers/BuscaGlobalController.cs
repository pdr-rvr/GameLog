using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class BuscaGlobalController : ControllerBase
    {
        private readonly GameLogContext _context;

        public BuscaGlobalController(GameLogContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Realiza busca global otimizada em Jogos, Usuários e Listas Públicas.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Buscar([FromQuery] string? q, [FromQuery] int limite = 5)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            {
                return Ok(new BuscaGlobalDTO { Termo = q?.Trim() ?? string.Empty });
            }

            var termo = q.Trim().ToLower();
            limite = Math.Clamp(limite, 1, 20);

            // 1. Busca em Jogos (Título, Descrição, Empresa, Gênero)
            var jogos = await _context.Jogos
                .AsNoTracking()
                .Include(j => j.Generos)
                .Include(j => j.Empresa)
                .Where(j => j.EstaAtivo && (
                    j.Titulo.ToLower().Contains(termo) ||
                    (j.Descricao != null && j.Descricao.ToLower().Contains(termo)) ||
                    (j.Empresa != null && j.Empresa.NomeEmpresa.ToLower().Contains(termo)) ||
                    j.Generos.Any(g => g.TituloGenero.ToLower().Contains(termo))
                ))
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
                        .Average(a => (double?)a.Nota)
                })
                .ToListAsync();

            // 2. Busca em Usuários (Nome de Usuário)
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

            // 3. Busca em Listas Públicas (Título, Descrição)
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

            var resultado = new BuscaGlobalDTO
            {
                Termo = q.Trim(),
                Jogos = jogos,
                Usuarios = usuarios,
                Listas = listas
            };

            return Ok(resultado);
        }
    }
}
