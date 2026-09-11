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
    public class BibliotecaServices
    {
        private readonly GameLogContext _context;

        public BibliotecaServices(GameLogContext context)
        {
            _context = context;
        }

        private static string ObterNomeStatus(StatusJogo status)
        {
            return status switch
            {
                StatusJogo.QueroJogar => "Quero Jogar",
                StatusJogo.Jogando => "Jogando",
                StatusJogo.Zerado => "Zerado",
                StatusJogo.Pausado => "Pausado",
                StatusJogo.Abandonado => "Abandonado",
                _ => status.ToString()
            };
        }

        public async Task<ItemBibliotecaDTO> SalvarItemBiblioteca(Guid usuarioId, SalvarItemBibliotecaDTO dto)
        {
            var jogo = await _context.Jogos.FindAsync(dto.JogoId);
            if (jogo == null || !jogo.EstaAtivo)
            {
                throw new KeyNotFoundException("Jogo não encontrado.");
            }

            var item = await _context.ItensBiblioteca
                .Include(b => b.Jogo)
                .ThenInclude(j => j.Empresa)
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId && b.JogoId == dto.JogoId);

            if (item == null)
            {
                item = new BibliotecaJogo
                {
                    UsuarioId = usuarioId,
                    JogoId = dto.JogoId,
                    Status = dto.Status,
                    DataAtualizacao = DateTime.UtcNow,
                    DataConclusao = dto.Status == StatusJogo.Zerado ? DateTime.UtcNow : null,
                    EstaAtivo = true
                };
                _context.ItensBiblioteca.Add(item);
            }
            else
            {
                item.Status = dto.Status;
                item.DataAtualizacao = DateTime.UtcNow;
                if (dto.Status == StatusJogo.Zerado && item.DataConclusao == null)
                {
                    item.DataConclusao = DateTime.UtcNow;
                }
                else if (dto.Status != StatusJogo.Zerado)
                {
                    item.DataConclusao = null;
                }
                item.EstaAtivo = true;
            }

            await _context.SaveChangesAsync();

            // Buscar nota do usuário se houver
            var avaliacaoUsuario = await _context.Avaliacoes
                .Where(a => a.Usuario.Id == usuarioId && a.Jogo.Id == dto.JogoId && a.EstaAtivo)
                .Select(a => (int?)a.Nota)
                .FirstOrDefaultAsync();

            var media = await _context.Avaliacoes
                .Where(a => a.Jogo.Id == dto.JogoId && a.EstaAtivo)
                .Select(a => (double?)a.Nota)
                .AverageAsync();

            return new ItemBibliotecaDTO
            {
                Id = item.Id,
                UsuarioId = item.UsuarioId,
                JogoId = item.JogoId,
                TituloJogo = jogo.Titulo,
                ImagemJogo = jogo.Imagem,
                NomeEmpresa = jogo.Empresa?.NomeEmpresa,
                EmpresaId = jogo.Empresa?.Id,
                DataLancamento = jogo.DataLancamento.ToString("yyyy-MM-dd"),
                Status = (int)item.Status,
                StatusNome = ObterNomeStatus(item.Status),
                DataAtualizacao = item.DataAtualizacao,
                DataConclusao = item.DataConclusao,
                MinhaNota = avaliacaoUsuario,
                MediaAvaliacoes = media.HasValue ? Math.Round(media.Value, 1) : null
            };
        }

        public async Task<ItemBibliotecaDTO?> ObterStatusJogo(Guid usuarioId, Guid jogoId)
        {
            var item = await _context.ItensBiblioteca
                .Include(b => b.Jogo)
                .ThenInclude(j => j.Empresa)
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId && b.JogoId == jogoId && b.EstaAtivo);

            if (item == null) return null;

            var avaliacaoUsuario = await _context.Avaliacoes
                .Where(a => a.Usuario.Id == usuarioId && a.Jogo.Id == jogoId && a.EstaAtivo)
                .Select(a => (int?)a.Nota)
                .FirstOrDefaultAsync();

            var media = await _context.Avaliacoes
                .Where(a => a.Jogo.Id == jogoId && a.EstaAtivo)
                .Select(a => (double?)a.Nota)
                .AverageAsync();

            return new ItemBibliotecaDTO
            {
                Id = item.Id,
                UsuarioId = item.UsuarioId,
                JogoId = item.JogoId,
                TituloJogo = item.Jogo.Titulo,
                ImagemJogo = item.Jogo.Imagem,
                NomeEmpresa = item.Jogo.Empresa?.NomeEmpresa,
                EmpresaId = item.Jogo.Empresa?.Id,
                DataLancamento = item.Jogo.DataLancamento.ToString("yyyy-MM-dd"),
                Status = (int)item.Status,
                StatusNome = ObterNomeStatus(item.Status),
                DataAtualizacao = item.DataAtualizacao,
                DataConclusao = item.DataConclusao,
                MinhaNota = avaliacaoUsuario,
                MediaAvaliacoes = media.HasValue ? Math.Round(media.Value, 1) : null
            };
        }

        public async Task<bool> RemoverDaBiblioteca(Guid usuarioId, Guid jogoId)
        {
            var item = await _context.ItensBiblioteca
                .FirstOrDefaultAsync(b => b.UsuarioId == usuarioId && b.JogoId == jogoId);

            if (item == null) return false;

            _context.ItensBiblioteca.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ItemBibliotecaDTO>> ListarBibliotecaUsuario(Guid usuarioId, StatusJogo? statusFiltro = null, string? busca = null)
        {
            var query = _context.ItensBiblioteca
                .Include(b => b.Jogo)
                .ThenInclude(j => j.Empresa)
                .Where(b => b.UsuarioId == usuarioId && b.EstaAtivo);

            if (statusFiltro.HasValue)
            {
                query = query.Where(b => b.Status == statusFiltro.Value);
            }

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                query = query.Where(b => b.Jogo.Titulo.ToLower().Contains(termo) 
                                      || (b.Jogo.Empresa != null && b.Jogo.Empresa.NomeEmpresa.ToLower().Contains(termo)));
            }

            var itens = await query
                .OrderByDescending(b => b.DataAtualizacao)
                .ToListAsync();

            // Buscar notas do usuário em lote
            var jogoIds = itens.Select(i => i.JogoId).Distinct().ToList();
            var avaliacoesDoUsuario = await _context.Avaliacoes
                .Where(a => a.Usuario.Id == usuarioId && jogoIds.Contains(a.Jogo.Id) && a.EstaAtivo)
                .ToDictionaryAsync(a => a.Jogo.Id, a => a.Nota);

            // Médias gerais dos jogos em lote
            var mediasGerais = await _context.Avaliacoes
                .Where(a => jogoIds.Contains(a.Jogo.Id) && a.EstaAtivo)
                .GroupBy(a => a.Jogo.Id)
                .Select(g => new { JogoId = g.Key, Media = g.Average(a => a.Nota) })
                .ToDictionaryAsync(g => g.JogoId, g => g.Media);

            return itens.Select(item => new ItemBibliotecaDTO
            {
                Id = item.Id,
                UsuarioId = item.UsuarioId,
                JogoId = item.JogoId,
                TituloJogo = item.Jogo.Titulo,
                ImagemJogo = item.Jogo.Imagem,
                NomeEmpresa = item.Jogo.Empresa?.NomeEmpresa,
                EmpresaId = item.Jogo.Empresa?.Id,
                DataLancamento = item.Jogo.DataLancamento.ToString("yyyy-MM-dd"),
                Status = (int)item.Status,
                StatusNome = ObterNomeStatus(item.Status),
                DataAtualizacao = item.DataAtualizacao,
                DataConclusao = item.DataConclusao,
                MinhaNota = avaliacoesDoUsuario.TryGetValue(item.JogoId, out var nota) ? nota : (int?)null,
                MediaAvaliacoes = mediasGerais.TryGetValue(item.JogoId, out var media) ? Math.Round(media, 1) : (double?)null
            }).ToList();
        }

        public async Task<EstatisticasBibliotecaDTO> ObterEstatisticasBiblioteca(Guid usuarioId)
        {
            var itens = await _context.ItensBiblioteca
                .Where(b => b.UsuarioId == usuarioId && b.EstaAtivo)
                .Select(b => b.Status)
                .ToListAsync();

            return new EstatisticasBibliotecaDTO
            {
                TotalJogos = itens.Count,
                TotalQueroJogar = itens.Count(s => s == StatusJogo.QueroJogar),
                TotalJogando = itens.Count(s => s == StatusJogo.Jogando),
                TotalZerados = itens.Count(s => s == StatusJogo.Zerado),
                TotalPausados = itens.Count(s => s == StatusJogo.Pausado),
                TotalAbandonados = itens.Count(s => s == StatusJogo.Abandonado)
            };
        }

        public async Task<List<JogoFavoritoDTO>> ObterJogosFavoritos(Guid usuarioId)
        {
            var favoritos = await _context.JogosFavoritosUsuarios
                .Include(f => f.Jogo)
                .ThenInclude(j => j.Empresa)
                .Where(f => f.UsuarioId == usuarioId && f.EstaAtivo)
                .OrderBy(f => f.Posicao)
                .ToListAsync();

            var jogoIds = favoritos.Select(f => f.JogoId).Distinct().ToList();
            var mediasGerais = await _context.Avaliacoes
                .Where(a => jogoIds.Contains(a.Jogo.Id) && a.EstaAtivo)
                .GroupBy(a => a.Jogo.Id)
                .Select(g => new { JogoId = g.Key, Media = g.Average(a => a.Nota) })
                .ToDictionaryAsync(g => g.JogoId, g => g.Media);

            return favoritos.Select(f => new JogoFavoritoDTO
            {
                Posicao = f.Posicao,
                JogoId = f.JogoId,
                TituloJogo = f.Jogo.Titulo,
                ImagemJogo = f.Jogo.Imagem,
                NomeEmpresa = f.Jogo.Empresa?.NomeEmpresa,
                EmpresaId = f.Jogo.Empresa?.Id,
                DataLancamento = f.Jogo.DataLancamento.ToString("yyyy-MM-dd"),
                MediaAvaliacoes = mediasGerais.TryGetValue(f.JogoId, out var media) ? Math.Round(media, 1) : (double?)null
            }).ToList();
        }

        public async Task<List<JogoFavoritoDTO>> SalvarJogosFavoritos(Guid usuarioId, SalvarJogosFavoritosDTO dto)
        {
            // Remover favoritos existentes do usuário
            var favoritosAtuais = await _context.JogosFavoritosUsuarios
                .Where(f => f.UsuarioId == usuarioId)
                .ToListAsync();

            _context.JogosFavoritosUsuarios.RemoveRange(favoritosAtuais);

            // Inserir novos favoritos validados (máximo 5 posições de 1 a 5)
            if (dto.Favoritos != null && dto.Favoritos.Count > 0)
            {
                var vistos = new HashSet<Guid>();
                var posicoesVistas = new HashSet<int>();

                foreach (var item in dto.Favoritos)
                {
                    if (item.Posicao < 1 || item.Posicao > 5) continue;
                    if (posicoesVistas.Contains(item.Posicao) || vistos.Contains(item.JogoId)) continue;

                    var jogoExiste = await _context.Jogos.AnyAsync(j => j.Id == item.JogoId && j.EstaAtivo);
                    if (!jogoExiste) continue;

                    _context.JogosFavoritosUsuarios.Add(new JogoFavoritoUsuario
                    {
                        UsuarioId = usuarioId,
                        JogoId = item.JogoId,
                        Posicao = item.Posicao,
                        EstaAtivo = true
                    });

                    vistos.Add(item.JogoId);
                    posicoesVistas.Add(item.Posicao);
                }
            }

            await _context.SaveChangesAsync();
            return await ObterJogosFavoritos(usuarioId);
        }
    }
}
