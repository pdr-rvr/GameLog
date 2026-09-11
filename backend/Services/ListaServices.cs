using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameLog_Backend.Services
{
    public class ListaServices
    {
        private readonly GameLogContext _context;

        public ListaServices(GameLogContext context)
        {
            _context = context;
        }

        public async Task<ListaDeJogosDTO> CriarLista(Guid usuarioId, CriarListaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Titulo))
            {
                throw new ArgumentException("O título da lista é obrigatório.");
            }

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null || !usuario.EstaAtivo)
            {
                throw new KeyNotFoundException("Usuário não encontrado.");
            }

            var lista = new ListaDeJogos
            {
                UsuarioId = usuarioId,
                Titulo = dto.Titulo.Trim(),
                Descricao = dto.Descricao?.Trim(),
                EstaPublica = dto.EstaPublica,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow,
                EstaAtivo = true
            };

            _context.Set<ListaDeJogos>().Add(lista);
            await _context.SaveChangesAsync();

            if (dto.JogosIds != null && dto.JogosIds.Any())
            {
                var ordem = 1;
                var jogosExistentes = await _context.Jogos
                    .Where(j => dto.JogosIds.Contains(j.Id) && j.EstaAtivo)
                    .Select(j => j.Id)
                    .ToListAsync();

                foreach (var jogoId in dto.JogosIds.Distinct())
                {
                    if (jogosExistentes.Contains(jogoId))
                    {
                        _context.Set<ItemDeLista>().Add(new ItemDeLista
                        {
                            ListaDeJogosId = lista.Id,
                            JogoId = jogoId,
                            Ordem = ordem++,
                            DataAdicionado = DateTime.UtcNow,
                            EstaAtivo = true
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            return (await ObterListaPorId(lista.Id, usuarioId))!;
        }

        public async Task<ListaDeJogosDTO?> ObterListaPorId(Guid listaId, Guid? usuarioAutenticadoId = null)
        {
            var lista = await _context.Set<ListaDeJogos>()
                .Include(l => l.Usuario)
                .Include(l => l.Itens)
                    .ThenInclude(i => i.Jogo)
                        .ThenInclude(j => j.Empresa)
                .FirstOrDefaultAsync(l => l.Id == listaId && l.EstaAtivo);

            if (lista == null) return null;

            // Se for privada e o solicitante não for o dono, não exibe
            if (!lista.EstaPublica && (!usuarioAutenticadoId.HasValue || usuarioAutenticadoId.Value != lista.UsuarioId))
            {
                return null;
            }

            var itensAtivos = lista.Itens
                .Where(i => i.EstaAtivo && i.Jogo != null && i.Jogo.EstaAtivo)
                .OrderBy(i => i.Ordem)
                .ToList();

            var jogoIds = itensAtivos.Select(i => i.JogoId).Distinct().ToList();

            var mediasGerais = await _context.Avaliacoes
                .Where(a => jogoIds.Contains(a.Jogo.Id) && a.EstaAtivo)
                .GroupBy(a => a.Jogo.Id)
                .Select(g => new { JogoId = g.Key, Media = g.Average(a => a.Nota) })
                .ToDictionaryAsync(g => g.JogoId, g => g.Media);

            var itensDTO = itensAtivos.Select(i => new ItemListaDTO
            {
                ItemId = i.Id,
                JogoId = i.JogoId,
                TituloJogo = i.Jogo.Titulo,
                ImagemJogo = i.Jogo.Imagem,
                NomeEmpresa = i.Jogo.Empresa?.NomeEmpresa,
                EmpresaId = i.Jogo.Empresa?.Id,
                DataLancamento = i.Jogo.DataLancamento.ToString("yyyy-MM-dd"),
                MediaAvaliacoes = mediasGerais.TryGetValue(i.JogoId, out var media) ? Math.Round(media, 1) : null,
                Ordem = i.Ordem,
                DataAdicionado = i.DataAdicionado
            }).ToList();

            var capasPreview = itensDTO
                .Where(i => !string.IsNullOrEmpty(i.ImagemJogo))
                .Select(i => i.ImagemJogo!)
                .Take(4)
                .ToList();

            return new ListaDeJogosDTO
            {
                ListaId = lista.Id,
                UsuarioId = lista.UsuarioId,
                NomeUsuario = lista.Usuario.NomeUsuario,
                FotoPerfilUsuario = lista.Usuario.FotoDePerfil,
                Titulo = lista.Titulo,
                Descricao = lista.Descricao,
                EstaPublica = lista.EstaPublica,
                DataCriacao = lista.DataCriacao,
                DataAtualizacao = lista.DataAtualizacao,
                TotalJogos = itensDTO.Count,
                CapasPreview = capasPreview,
                Itens = itensDTO
            };
        }

        public async Task<List<ListaDeJogosDTO>> ListarListasDoUsuario(Guid usuarioId, Guid? usuarioAutenticadoId = null)
        {
            var isDono = usuarioAutenticadoId.HasValue && usuarioAutenticadoId.Value == usuarioId;

            var listas = await _context.Set<ListaDeJogos>()
                .Include(l => l.Usuario)
                .Include(l => l.Itens)
                    .ThenInclude(i => i.Jogo)
                .Where(l => l.UsuarioId == usuarioId && l.EstaAtivo && (isDono || l.EstaPublica))
                .OrderByDescending(l => l.DataAtualizacao)
                .ToListAsync();

            return listas.Select(l =>
            {
                var itensAtivos = l.Itens
                    .Where(i => i.EstaAtivo && i.Jogo != null && i.Jogo.EstaAtivo)
                    .OrderBy(i => i.Ordem)
                    .ToList();

                var capas = itensAtivos
                    .Where(i => !string.IsNullOrEmpty(i.Jogo.Imagem))
                    .Select(i => i.Jogo.Imagem)
                    .Take(4)
                    .ToList();

                return new ListaDeJogosDTO
                {
                    ListaId = l.Id,
                    UsuarioId = l.UsuarioId,
                    NomeUsuario = l.Usuario.NomeUsuario,
                    FotoPerfilUsuario = l.Usuario.FotoDePerfil,
                    Titulo = l.Titulo,
                    Descricao = l.Descricao,
                    EstaPublica = l.EstaPublica,
                    DataCriacao = l.DataCriacao,
                    DataAtualizacao = l.DataAtualizacao,
                    TotalJogos = itensAtivos.Count,
                    CapasPreview = capas,
                    Itens = new List<ItemListaDTO>() // Itens completos são retornados no ObterPorId
                };
            }).ToList();
        }

        public async Task<ListaDeJogosDTO?> EditarLista(Guid listaId, Guid usuarioId, EditarListaDTO dto)
        {
            var lista = await _context.Set<ListaDeJogos>()
                .Include(l => l.Itens)
                .FirstOrDefaultAsync(l => l.Id == listaId && l.UsuarioId == usuarioId && l.EstaAtivo);

            if (lista == null) return null;

            if (string.IsNullOrWhiteSpace(dto.Titulo))
            {
                throw new ArgumentException("O título da lista não pode ser vazio.");
            }

            lista.Titulo = dto.Titulo.Trim();
            lista.Descricao = dto.Descricao?.Trim();
            lista.EstaPublica = dto.EstaPublica;
            lista.DataAtualizacao = DateTime.UtcNow;

            if (dto.JogosIds != null)
            {
                // Remover itens antigos
                _context.Set<ItemDeLista>().RemoveRange(lista.Itens);

                var ordem = 1;
                var jogosExistentes = await _context.Jogos
                    .Where(j => dto.JogosIds.Contains(j.Id) && j.EstaAtivo)
                    .Select(j => j.Id)
                    .ToListAsync();

                foreach (var jogoId in dto.JogosIds.Distinct())
                {
                    if (jogosExistentes.Contains(jogoId))
                    {
                        _context.Set<ItemDeLista>().Add(new ItemDeLista
                        {
                            ListaDeJogosId = lista.Id,
                            JogoId = jogoId,
                            Ordem = ordem++,
                            DataAdicionado = DateTime.UtcNow,
                            EstaAtivo = true
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return await ObterListaPorId(listaId, usuarioId);
        }

        public async Task<bool> DeletarLista(Guid listaId, Guid usuarioId)
        {
            var lista = await _context.Set<ListaDeJogos>()
                .FirstOrDefaultAsync(l => l.Id == listaId && l.UsuarioId == usuarioId && l.EstaAtivo);

            if (lista == null) return false;

            lista.EstaAtivo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AdicionarJogoNaLista(Guid listaId, Guid usuarioId, Guid jogoId)
        {
            var lista = await _context.Set<ListaDeJogos>()
                .Include(l => l.Itens)
                .FirstOrDefaultAsync(l => l.Id == listaId && l.UsuarioId == usuarioId && l.EstaAtivo);

            if (lista == null) return false;

            var jogoExiste = await _context.Jogos.AnyAsync(j => j.Id == jogoId && j.EstaAtivo);
            if (!jogoExiste) throw new KeyNotFoundException("Jogo não encontrado.");

            var itemJaExiste = lista.Itens.Any(i => i.JogoId == jogoId && i.EstaAtivo);
            if (itemJaExiste) return true; // Já está na lista

            var proximaOrdem = lista.Itens.Where(i => i.EstaAtivo).Select(i => i.Ordem).DefaultIfEmpty(0).Max() + 1;

            _context.Set<ItemDeLista>().Add(new ItemDeLista
            {
                ListaDeJogosId = listaId,
                JogoId = jogoId,
                Ordem = proximaOrdem,
                DataAdicionado = DateTime.UtcNow,
                EstaAtivo = true
            });

            lista.DataAtualizacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoverJogoDaLista(Guid listaId, Guid usuarioId, Guid jogoId)
        {
            var lista = await _context.Set<ListaDeJogos>()
                .FirstOrDefaultAsync(l => l.Id == listaId && l.UsuarioId == usuarioId && l.EstaAtivo);

            if (lista == null) return false;

            var item = await _context.Set<ItemDeLista>()
                .FirstOrDefaultAsync(i => i.ListaDeJogosId == listaId && i.JogoId == jogoId);

            if (item == null) return false;

            _context.Set<ItemDeLista>().Remove(item);
            lista.DataAtualizacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
