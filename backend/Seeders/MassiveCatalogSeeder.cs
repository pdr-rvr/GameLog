using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;
using GameLog_Backend.Helpers;
using GameLog_Backend.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GameLog_Backend.Seeders
{
    public class MassiveCatalogSeeder
    {
        private readonly GameLogContext _context;
        private readonly RawgApiService _rawgService;
        private readonly ILogger<MassiveCatalogSeeder> _logger;

        public MassiveCatalogSeeder(
            GameLogContext context, 
            RawgApiService rawgService,
            ILogger<MassiveCatalogSeeder> logger)
        {
            _context = context;
            _rawgService = rawgService;
            _logger = logger;
        }

        public async Task NormalizarGenerosExistentesAsync()
        {
            try
            {
                _logger.LogInformation("[Normalizador] Iniciando consolidação e normalização de gêneros...");
                var canonicalMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Action", "Ação" },
                    { "Acao", "Ação" },
                    { "Aao", "Ação" },
                    { "Adventure", "Aventura" },
                    { "Role-Playing Games", "RPG" },
                    { "Role Playing", "RPG" },
                    { "Shooter", "Tiro" },
                    { "Strategy", "Estratégia" },
                    { "Estrategia", "Estratégia" },
                    { "Estratgia", "Estratégia" },
                    { "Racing", "Corrida" },
                    { "Sports", "Esportes" },
                    { "Fighting", "Luta" },
                    { "Puzzle", "Quebra-Cabeça" },
                    { "Quebra-Cabeca", "Quebra-Cabeça" },
                    { "Quebra-Cabea", "Quebra-Cabeça" },
                    { "Simulation", "Simulação" },
                    { "Simulacao", "Simulação" },
                    { "Simulaao", "Simulação" },
                    { "Horror", "Terror & Sobrevivência" },
                    { "Survival", "Terror & Sobrevivência" },
                    { "Terror e Sobrevivência", "Terror & Sobrevivência" },
                    { "Terror & Sobrevivncia", "Terror & Sobrevivência" },
                    { "Platformer", "Plataforma" },
                    { "Platform", "Plataforma" },
                    { "Massively Multiplayer", "MMO" }
                };

                var todosGeneros = await _context.Generos.Include(g => g.Jogos).ToListAsync();
                var generosPorNome = new Dictionary<string, Genero>(StringComparer.OrdinalIgnoreCase);

                foreach (var g in todosGeneros)
                {
                    if (!canonicalMap.ContainsKey(g.TituloGenero))
                    {
                        generosPorNome[g.TituloGenero] = g;
                    }
                }

                foreach (var g in todosGeneros)
                {
                    if (canonicalMap.TryGetValue(g.TituloGenero, out var targetName))
                    {
                        if (!generosPorNome.TryGetValue(targetName, out var targetGen))
                        {
                            targetGen = await _context.Generos.FirstOrDefaultAsync(x => x.TituloGenero == targetName);
                            if (targetGen == null)
                            {
                                targetGen = new Genero { TituloGenero = targetName, EstaAtivo = true };
                                _context.Generos.Add(targetGen);
                                await _context.SaveChangesAsync();
                            }
                            generosPorNome[targetName] = targetGen;
                        }

                        // Transferir referências de jogos
                        foreach (var jogo in g.Jogos.ToList())
                        {
                            if (!jogo.Generos.Any(x => x.Id == targetGen.Id))
                            {
                                jogo.Generos.Add(targetGen);
                            }
                            jogo.Generos.Remove(g);
                        }

                        g.EstaAtivo = false;
                    }
                }

                await _context.SaveChangesAsync();

                // Limpar gêneros inativos órfãos
                var inativos = await _context.Generos.Where(g => !g.EstaAtivo && !g.Jogos.Any()).ToListAsync();
                if (inativos.Any())
                {
                    _context.Generos.RemoveRange(inativos);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation("[Normalizador] Normalização concluída. Gêneros consolidados no padrão oficial.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Normalizador] Falha durante normalização de gêneros legados.");
            }
        }

        public async Task ConsolidarENormalizarEmpresasExistentesAsync()
        {
            try
            {
                _logger.LogInformation("[Normalizador] Iniciando consolidação e normalização de empresas e estúdios...");

                var todasEmpresas = await _context.Empresa.ToListAsync();
                var todosJogos = await _context.Jogos
                    .Include(j => j.Empresa)
                    .Include(j => j.Publicadora)
                    .ToListAsync();

                var empresasPorNome = new Dictionary<string, Empresa>(StringComparer.OrdinalIgnoreCase);

                foreach (var emp in todasEmpresas)
                {
                    var nomeCan = NormalizadorEmpresaHelper.NormalizarNomeEmpresa(emp.NomeEmpresa);
                    if (string.Equals(emp.NomeEmpresa, nomeCan, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!empresasPorNome.ContainsKey(nomeCan))
                        {
                            empresasPorNome[nomeCan] = emp;
                        }
                    }
                }

                foreach (var emp in todasEmpresas)
                {
                    var nomeCan = NormalizadorEmpresaHelper.NormalizarNomeEmpresa(emp.NomeEmpresa);
                    if (!empresasPorNome.TryGetValue(nomeCan, out var empCan))
                    {
                        empCan = todasEmpresas.FirstOrDefault(e => string.Equals(e.NomeEmpresa, nomeCan, StringComparison.OrdinalIgnoreCase));
                        if (empCan == null)
                        {
                            empCan = new Empresa { NomeEmpresa = nomeCan, EstaAtivo = true };
                            _context.Empresa.Add(empCan);
                            await _context.SaveChangesAsync();
                        }
                        empresasPorNome[nomeCan] = empCan;
                    }
                }

                foreach (var jogo in todosJogos)
                {
                    var devAtual = jogo.Empresa?.NomeEmpresa;
                    var pubAtual = jogo.Publicadora?.NomeEmpresa;

                    var (devResolvido, pubResolvido) = NormalizadorEmpresaHelper.ResolverParDesenvolvedoraPublicadora(jogo.Titulo, devAtual, pubAtual);

                    var devCan = NormalizadorEmpresaHelper.NormalizarNomeEmpresa(devResolvido);
                    if (!empresasPorNome.TryGetValue(devCan, out var targetDev))
                    {
                        targetDev = new Empresa { NomeEmpresa = devCan, EstaAtivo = true };
                        _context.Empresa.Add(targetDev);
                        await _context.SaveChangesAsync();
                        empresasPorNome[devCan] = targetDev;
                    }
                    jogo.Empresa = targetDev;

                    if (!string.IsNullOrWhiteSpace(pubResolvido))
                    {
                        var pubCan = NormalizadorEmpresaHelper.NormalizarNomeEmpresa(pubResolvido);
                        if (!empresasPorNome.TryGetValue(pubCan, out var targetPub))
                        {
                            targetPub = new Empresa { NomeEmpresa = pubCan, EstaAtivo = true };
                            _context.Empresa.Add(targetPub);
                            await _context.SaveChangesAsync();
                            empresasPorNome[pubCan] = targetPub;
                        }
                        jogo.Publicadora = targetPub;
                    }
                    else
                    {
                        jogo.Publicadora = null;
                    }
                }

                await _context.SaveChangesAsync();

                var empresasEmUsoIds = new HashSet<Guid>(
                    todosJogos.Where(j => j.Empresa != null).Select(j => j.Empresa.Id)
                    .Concat(todosJogos.Where(j => j.Publicadora != null).Select(j => j.Publicadora!.Id))
                );

                var orfas = todasEmpresas.Where(e => !empresasEmUsoIds.Contains(e.Id)).ToList();
                if (orfas.Any())
                {
                    _context.Empresa.RemoveRange(orfas);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("[Normalizador] Removidas {Total} empresas redundantes/órfãs do banco.", orfas.Count);
                }

                _logger.LogInformation("[Normalizador] Consolidação de empresas e estúdios concluída com sucesso.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Normalizador] Falha durante consolidação de empresas.");
            }
        }

        public async Task CleanAndSeedRealGamesAsync()
        {
            var totalJogosExistentes = await _context.Jogos.CountAsync();
            if (totalJogosExistentes >= 2400)
            {
                _logger.LogInformation("[Seeder] Catálogo já se encontra totalmente povoado com {Total} jogos oficiais. Pulando re-seed...", totalJogosExistentes);
                await NormalizarGenerosExistentesAsync();
                await ConsolidarENormalizarEmpresasExistentesAsync();
                return;
            }

            _logger.LogInformation("[Seeder] Iniciando limpeza completa do banco de dados para ingestão curada de 2.500 jogos oficiais (Recentes 2022-2026: 700, Era 2000-2021: 1.700, Clássicos Pré-2000: 100)...");

            // 1. Limpeza segura e completa do banco de dados
            try
            {
                await _context.Database.ExecuteSqlRawAsync(@"
                    DELETE FROM SegueUsuarios;
                    DELETE FROM ItensDeListas;
                    DELETE FROM ListasDeJogos;
                    DELETE FROM JogosFavoritosUsuarios;
                    DELETE FROM ItensBiblioteca;
                    DELETE FROM CurtidasDeRespostas;
                    DELETE FROM RespostasDeAvaliacao;
                    DELETE FROM CurtidasDeAvaliacoes;
                    DELETE FROM Avaliacoes;
                    DELETE FROM JogoGenero;
                    DELETE FROM Jogos;
                    DELETE FROM Empresa;
                    DELETE FROM Generos;
                    DELETE FROM Usuarios;
                    DBCC CHECKIDENT ('Jogos', RESEED, 0);
                    DBCC CHECKIDENT ('Empresa', RESEED, 0);
                    DBCC CHECKIDENT ('Generos', RESEED, 0);
                    DBCC CHECKIDENT ('Usuarios', RESEED, 0);
                ");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Seeder] Falha na limpeza direta via SQL; executando limpeza via EF Core...");
                _context.SegueUsuarios.RemoveRange(_context.SegueUsuarios);
                _context.ItensDeListas.RemoveRange(_context.ItensDeListas);
                _context.ListasDeJogos.RemoveRange(_context.ListasDeJogos);
                _context.JogosFavoritosUsuarios.RemoveRange(_context.JogosFavoritosUsuarios);
                _context.ItensBiblioteca.RemoveRange(_context.ItensBiblioteca);
                _context.CurtidasDeRespostas.RemoveRange(_context.CurtidasDeRespostas);
                _context.RespostasDeAvaliacao.RemoveRange(_context.RespostasDeAvaliacao);
                _context.CurtidasDeAvaliacoes.RemoveRange(_context.CurtidasDeAvaliacoes);
                _context.Avaliacoes.RemoveRange(_context.Avaliacoes);
                _context.Jogos.RemoveRange(_context.Jogos);
                _context.Empresa.RemoveRange(_context.Empresa);
                _context.Generos.RemoveRange(_context.Generos);
                _context.Usuarios.RemoveRange(_context.Usuarios);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("[Seeder] Banco de dados resetado com sucesso.");

            // 2. Criar Gêneros Oficiais Fundamentais
            var generosNomes = new[]
            {
                "Ação", "RPG", "Aventura", "Tiro", "Estratégia",
                "Terror & Sobrevivência", "Plataforma", "Corrida", "Luta",
                "Metroidvania", "Roguelike", "Hack and Slash", "Simulação",
                "Mundo Aberto", "Quebra-Cabeça", "Indie",
                "Esportes", "Stealth", "Cyberpunk", "Fantasia Sombria", "Casual"
            };

            foreach (var nome in generosNomes)
            {
                _context.Generos.Add(new Genero { TituloGenero = nome, EstaAtivo = true });
            }
            await _context.SaveChangesAsync();
            var generosDb = await _context.Generos.AsNoTracking().ToListAsync();
            var generoDict = generosDb.ToDictionary(g => g.TituloGenero, g => g.Id, StringComparer.OrdinalIgnoreCase);
            var defaultGeneroId = generoDict["Ação"];

            // 3. Criar Empresas / Estúdios Fundamentais no Banco
            var empresasIniciais = ObterListaEstudiosFundamentais();
            foreach (var nome in empresasIniciais)
            {
                _context.Empresa.Add(new Empresa { NomeEmpresa = nome, EstaAtivo = true });
            }
            await _context.SaveChangesAsync();

            var empresasDb = await _context.Empresa.AsNoTracking().ToListAsync();
            var empresaDict = new ConcurrentDictionary<string, Guid>(
                empresasDb.ToDictionary(e => e.NomeEmpresa, e => e.Id, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase
            );

            // 4. Ingestão Curada: Top All-Time + Sucessos Recentes (2023-2026) + Clássicos Pré-2000 + Busca Ativa de Franquias
            _logger.LogInformation("[Seeder] Iniciando download curado da RAWG API com ordenação global de popularidade...");

            var jogosRecentes2022a2026 = new ConcurrentBag<RawgGameItemDTO>();
            var jogosGerais2000a2021 = new ConcurrentBag<RawgGameItemDTO>();
            var jogosClassicosPre2000 = new ConcurrentBag<RawgGameItemDTO>();
            var chavesNormalizadasVistas = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
            var slugsVistos = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

            void ProcessarItem(RawgGameItemDTO item, bool ehClassicoExplicit = false)
            {
                if (item == null ||
                    string.IsNullOrWhiteSpace(item.Name) ||
                    string.IsNullOrWhiteSpace(item.BackgroundImage) ||
                    !item.BackgroundImage.StartsWith("http") ||
                    !RawgApiService.EhJogoValido(item.Name))
                {
                    return;
                }

                // Filtro Rígido de Relevância Comercial: Descartar projetos amadores, protótipos de game jams e clones
                if ((item.Added ?? 0) < 30 && !item.Metacritic.HasValue)
                {
                    return;
                }

                var chaveNorm = NormalizarTituloParaDeduplicacao(item.Name);
                if (string.IsNullOrWhiteSpace(chaveNorm)) return;

                var slugNorm = (item.Slug ?? string.Empty).Trim().ToLowerInvariant();

                if (chavesNormalizadasVistas.TryAdd(chaveNorm, 1))
                {
                    if (!string.IsNullOrEmpty(slugNorm))
                    {
                        slugsVistos.TryAdd(slugNorm, 1);
                    }

                    int ano = 2020;
                    if (!string.IsNullOrWhiteSpace(item.Released) && DateTime.TryParse(item.Released, out var dt))
                    {
                        ano = dt.Year;
                    }

                    if (ehClassicoExplicit || ano < 2000)
                    {
                        jogosClassicosPre2000.Add(item);
                    }
                    else if (ano >= 2022)
                    {
                        jogosRecentes2022a2026.Add(item);
                    }
                    else
                    {
                        jogosGerais2000a2021.Add(item);
                    }
                }
            }

            // 1. Ingestão Global por Popularidade Real (ordering=-added) com Slices Recentes e Clássicos
            var slicesPrincipais = new (string Query, int Paginas, bool EhClassico)[]
            {
                // Slices de Anos Recentes (2022 a 2026) com ordenação por popularidade
                ("dates=2022-01-01,2022-12-31&ordering=-added", 15, false),         // ~600 sucessos de 2022
                ("dates=2023-01-01,2023-12-31&ordering=-added", 15, false),         // ~600 sucessos de 2023
                ("dates=2024-01-01,2024-12-31&ordering=-added", 15, false),         // ~600 sucessos de 2024
                ("dates=2025-01-01,2026-12-31&ordering=-added", 15, false),         // ~600 lançamentos de 2025/2026

                // Sucessos All-Time e Era Dourada Pós-2000
                ("ordering=-added", 70, false),                                     // ~2.800 maiores sucessos mundiais de todos os tempos
                ("dates=2000-01-01,2021-12-31&ordering=-added", 30, false),         // ~1.200 grandes sucessos de 2000 a 2021
                ("platforms=7,18,1,186,187&ordering=-added", 25, false),            // ~1.000 sucessos aclamados de consoles e PC

                // Clássicos Atemporais Pré-2000
                ("dates=1980-01-01,1999-12-31&ordering=-added", 25, true),          // ~1.000 clássicos atemporais pré-2000
                ("dates=1980-01-01,1999-12-31&ordering=-rating", 10, true)          // ~400 clássicos com maiores avaliações
            };

            using var semaphore = new SemaphoreSlim(8);

            var tarefasPrincipais = slicesPrincipais.Select(async slice =>
            {
                var tarefasPaginas = Enumerable.Range(1, slice.Paginas).Select(async p =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var results = await _rawgService.ObterJogosRawgQueryAsync(slice.Query, p, 40);
                        foreach (var item in results)
                        {
                            ProcessarItem(item, slice.EhClassico);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("[Seeder] Falha ao consultar {Query} página {Pagina}: {Msg}", slice.Query, p, ex.Message);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tarefasPaginas);
            });

            await Task.WhenAll(tarefasPrincipais);

            // 2. Cobertura Curada das Maiores Franquias Históricas com Limitação Estrita por Título Canônico
            var franquiasIconicas = new (string Nome, int Limite)[]
            {
                ("The Legend of Zelda", 8), ("Super Mario", 10), ("Metroid", 6), ("Pokemon", 10), 
                ("Castlevania", 6), ("Final Fantasy", 12), ("Dragon Quest", 8), ("Persona", 8), 
                ("Silent Hill", 6), ("Resident Evil", 10), ("God of War", 6), ("Dark Souls", 6), 
                ("Grand Theft Auto", 8), ("Halo", 6), ("Chrono Trigger", 3), ("Kingdom Hearts", 6), 
                ("Monster Hunter", 6), ("Street Fighter", 6), ("Tekken", 6), ("Donkey Kong", 6), 
                ("Crash Bandicoot", 6), ("Spyro", 4), ("Sonic the Hedgehog", 8), ("Mega Man", 6), 
                ("Metal Gear Solid", 6), ("BioShock", 4), ("Half-Life", 4), ("Dead Space", 4),
                ("The Witcher", 4), ("Red Dead Redemption", 3), ("Mass Effect", 4), ("Dragon Age", 4),
                ("Fallout", 5), ("The Elder Scrolls", 5), ("Assassin's Creed", 8), ("Far Cry", 6),
                ("Tomb Raider", 6), ("Uncharted", 5), ("The Last of Us", 3), ("Devil May Cry", 5),
                ("Yakuza", 8), ("Kingdom Come: Deliverance", 2), ("Hollow Knight", 2), ("Hades", 2),
                ("DOOM", 5), ("Wolfenstein", 5), ("Borderlands", 5), ("Diablo", 4),
                ("Portal", 2), ("Dishonored", 3), ("Batman: Arkham", 4), ("Mortal Kombat", 6),
                ("Need for Speed", 6), ("Gran Turismo", 5), ("Forza", 5), ("Call of Duty", 8),
                ("Battlefield", 6), ("Hitman", 5), ("Splinter Cell", 4), ("Civilization", 4),
                ("Age of Empires", 4), ("StarCraft", 3), ("Warcraft", 3), ("Baldur's Gate", 4),
                ("Alan Wake", 3), ("Life is Strange", 4), ("Max Payne", 3), ("Star Wars Jedi", 3),
                ("NieR", 3), ("Tales of", 6), ("Xenoblade Chronicles", 4), ("Fire Emblem", 5),
                ("Kirby", 5), ("Super Smash Bros", 4), ("Rayman", 4), ("Prince of Persia", 4),
                ("Deus Ex", 4), ("Crysis", 3), ("Metro", 4), ("S.T.A.L.K.E.R.", 4),
                ("PAYDAY", 2), ("Dying Light", 2), ("Watch Dogs", 3), ("Overwatch", 2),
                ("Little Nightmares", 2), ("Outlast", 3), ("Danganronpa", 4), ("Guilty Gear", 4)
            };

            _logger.LogInformation("[Seeder] Verificando e garantindo presença dos títulos canônicos de {Total} franquias históricas...", franquiasIconicas.Length);

            var tarefasFranquias = franquiasIconicas.Select(async f =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var results = await _rawgService.ObterJogosRawgQueryAsync($"search={Uri.EscapeDataString(f.Nome)}&search_precise=true&ordering=-added", 1, 20);
                    var topCanonicais = results
                        .Where(r => (r.Added ?? 0) >= 30 || r.Metacritic.HasValue)
                        .OrderByDescending(r => r.Added ?? 0)
                        .Take(f.Limite);

                    foreach (var item in topCanonicais)
                    {
                        ProcessarItem(item, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("[Seeder] Falha ao consultar franquia {Franquia}: {Msg}", f.Nome, ex.Message);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tarefasFranquias);

            // 3. Montar Catálogo Final Estratificado: Recentes (2022-2026: 700) + Geral (2000-2021: 1.700) + Clássicos Pré-2000 (100) = 2.500 Jogos
            var candidatosRecentes = jogosRecentes2022a2026
                .OrderByDescending(j => j.Added ?? 0)
                .ThenByDescending(j => j.Rating ?? 0)
                .ToList();

            var candidatosGerais = jogosGerais2000a2021
                .OrderByDescending(j => j.Added ?? 0)
                .ThenByDescending(j => j.Rating ?? 0)
                .ToList();

            var candidatosPre2000 = jogosClassicosPre2000
                .OrderByDescending(j => j.Added ?? 0)
                .ThenByDescending(j => j.Rating ?? 0)
                .ToList();

            _logger.LogInformation("[Seeder] Resolvendo estúdios para candidatos (Recentes 2022-2026: {TotalRec}, Gerais 2000-2021: {TotalGer}, Pré-2000: {TotalPre})...", 
                candidatosRecentes.Count, candidatosGerais.Count, candidatosPre2000.Count);

            using var detailSemaphore = new SemaphoreSlim(12);

            async Task<List<(RawgGameItemDTO Rawg, string StudioNome)>> ColetarJogosValidosAsync(List<RawgGameItemDTO> candidatos, int metaQtd)
            {
                var resultado = new List<(RawgGameItemDTO Rawg, string StudioNome)>();
                var titulosVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                const int loteTamanho = 50;
                for (int i = 0; i < candidatos.Count && resultado.Count < metaQtd; i += loteTamanho)
                {
                    var lote = candidatos.Skip(i).Take(loteTamanho).ToList();
                    var tarefasLote = lote.Select(async item =>
                    {
                        var titulo = FormatarTituloFinal(item.Name.Trim());
                        if (string.IsNullOrWhiteSpace(titulo)) return ((RawgGameItemDTO)null!, (string)null!);

                        var studio = await ResolverEstudioCompletoAsync(item, detailSemaphore);
                        if (string.IsNullOrWhiteSpace(studio)) return ((RawgGameItemDTO)null!, (string)null!);

                        return (item, studio);
                    });

                    var resultadosLote = await Task.WhenAll(tarefasLote);
                    foreach (var (rawg, studioNome) in resultadosLote)
                    {
                        if (rawg == null || string.IsNullOrWhiteSpace(studioNome)) continue;

                        var titulo = FormatarTituloFinal(rawg.Name.Trim());
                        if (titulo.Length > 250) titulo = titulo.Substring(0, 250).Trim();

                        if (titulosVistos.Add(titulo))
                        {
                            resultado.Add((rawg, studioNome));
                            if (resultado.Count >= metaQtd) break;
                        }
                    }
                }

                return resultado;
            }

            var jogosRecentesValidos = await ColetarJogosValidosAsync(candidatosRecentes, 700);
            var jogosGeraisValidos = await ColetarJogosValidosAsync(candidatosGerais, 1700);
            var jogosPre2000Validos = await ColetarJogosValidosAsync(candidatosPre2000, 100);

            var jogosComEstudioValido = new List<(RawgGameItemDTO Rawg, string StudioNome)>();
            jogosComEstudioValido.AddRange(jogosRecentesValidos);
            jogosComEstudioValido.AddRange(jogosGeraisValidos);
            jogosComEstudioValido.AddRange(jogosPre2000Validos);

            _logger.LogInformation("[Seeder] Total curado selecionado com estúdios 100% autênticos: {Total} jogos (Recentes 2022-2026: {Recentes}, Gerais 2000-2021: {Gerais}, Clássicos Pré-2000: {Classicos})!", 
                jogosComEstudioValido.Count, jogosRecentesValidos.Count, jogosGeraisValidos.Count, jogosPre2000Validos.Count);

            // 5. Inserir novas empresas descobertas dinamicamente
            var connectionString = _context.Database.GetConnectionString();
            var novasEmpresas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (_, studioNome) in jogosComEstudioValido)
            {
                if (!empresaDict.ContainsKey(studioNome))
                {
                    novasEmpresas.Add(studioNome);
                }
            }

            if (novasEmpresas.Any())
            {
                _logger.LogInformation("[Seeder] Cadastrando {Total} novos estúdios verificados no banco...", novasEmpresas.Count);
                foreach (var nomeEmp in novasEmpresas)
                {
                    var emp = new Empresa { NomeEmpresa = nomeEmp.Length > 150 ? nomeEmp.Substring(0, 150) : nomeEmp, EstaAtivo = true };
                    _context.Empresa.Add(emp);
                }
                await _context.SaveChangesAsync();

                var todasEmpresasAtualizadas = await _context.Empresa.AsNoTracking().ToListAsync();
                foreach (var emp in todasEmpresasAtualizadas)
                {
                    empresaDict[emp.NomeEmpresa] = emp.Id;
                }
            }

            // 6. Montar tabelas em memória para SqlBulkCopy com Garantia Absoluta Anti-Duplicação e Estúdios 100% Autênticos
            var tableJogos = new DataTable("Jogos");
            tableJogos.Columns.Add("JogoId", typeof(Guid));
            tableJogos.Columns.Add("Titulo", typeof(string));
            tableJogos.Columns.Add("Descricao", typeof(string));
            tableJogos.Columns.Add("Imagem", typeof(string));
            tableJogos.Columns.Add("DataLancamento", typeof(DateTime));
            tableJogos.Columns.Add("ClassificacaoIndicativa", typeof(int));
            tableJogos.Columns.Add("EmpresaId", typeof(Guid));
            tableJogos.Columns.Add("PublicadoraId", typeof(Guid));
            tableJogos.Columns.Add("EstaAtivo", typeof(bool));

            var tableJogoGenero = new DataTable("JogoGenero");
            tableJogoGenero.Columns.Add("GenerosId", typeof(Guid));
            tableJogoGenero.Columns.Add("JogosId", typeof(Guid));

            var titulosInseridosNaTabela = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var relacoesVistas = new HashSet<(Guid, Guid)>();

            foreach (var (rawg, studioNome) in jogosComEstudioValido)
            {
                var titulo = FormatarTituloFinal(rawg.Name.Trim());
                if (titulo.Length > 250) titulo = titulo.Substring(0, 250).Trim();

                // Garantia estrita de que nenhum título duplicado entrará no DataTable
                if (!titulosInseridosNaTabela.Add(titulo))
                {
                    continue;
                }

                if (!empresaDict.TryGetValue(studioNome, out var empId))
                {
                    continue;
                }

                Guid? pubId = null;
                var rawgPub = rawg.Publishers?.FirstOrDefault()?.Name;
                if (!string.IsNullOrWhiteSpace(rawgPub) && empresaDict.TryGetValue(rawgPub, out var foundPubId))
                {
                    pubId = foundPubId;
                }

                var dtLanc = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(rawg.Released) && DateTime.TryParse(rawg.Released, out var dtParsed))
                {
                    dtLanc = dtParsed;
                }

                int classif = 0;
                if (rawg.EsrbRating != null && !string.IsNullOrWhiteSpace(rawg.EsrbRating.Slug))
                {
                    classif = rawg.EsrbRating.Slug switch
                    {
                        "everyone" => 0,
                        "everyone-10-plus" => 10,
                        "teen" => 14,
                        "mature" => 18,
                        "adults-only" => 18,
                        _ => 0
                    };
                }

                // Resolução dos gêneros
                var (gensIds, gensNomes) = ResolverGeneros(rawg, generoDict, defaultGeneroId);

                // Geração de Descrição Rica e Narrativa (sem apenas repetir a nota)
                var desc = GerarDescricaoRica(titulo, rawg, studioNome, gensNomes, dtLanc);

                var jogoId = UuidV7Helper.NewGuid();
                tableJogos.Rows.Add(jogoId, titulo, desc, rawg.BackgroundImage, dtLanc, classif, empId, pubId.HasValue ? (object)pubId.Value : DBNull.Value, true);

                foreach (var gId in gensIds)
                {
                    if (relacoesVistas.Add((gId, jogoId)))
                    {
                        tableJogoGenero.Rows.Add(gId, jogoId);
                    }
                }
            }

            // 7. Bulk Copy no SQL Server
            _logger.LogInformation("[Seeder] Executando SqlBulkCopy para {Total} jogos curados e únicos...", tableJogos.Rows.Count);
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "dbo.Jogos";
                    bulkCopy.BulkCopyTimeout = 300;
                    bulkCopy.ColumnMappings.Add("JogoId", "JogoId");
                    bulkCopy.ColumnMappings.Add("Titulo", "Titulo");
                    bulkCopy.ColumnMappings.Add("Descricao", "Descricao");
                    bulkCopy.ColumnMappings.Add("Imagem", "Imagem");
                    bulkCopy.ColumnMappings.Add("DataLancamento", "DataLancamento");
                    bulkCopy.ColumnMappings.Add("ClassificacaoIndicativa", "ClassificacaoIndicativa");
                    bulkCopy.ColumnMappings.Add("EmpresaId", "EmpresaId");
                    bulkCopy.ColumnMappings.Add("PublicadoraId", "PublicadoraId");
                    bulkCopy.ColumnMappings.Add("EstaAtivo", "EstaAtivo");
                    await bulkCopy.WriteToServerAsync(tableJogos);
                }

                _logger.LogInformation("[Seeder] Executando SqlBulkCopy para {Total} relações JogoGenero...", tableJogoGenero.Rows.Count);
                using (var bulkCopyJG = new SqlBulkCopy(connection))
                {
                    bulkCopyJG.DestinationTableName = "dbo.JogoGenero";
                    bulkCopyJG.BulkCopyTimeout = 300;
                    bulkCopyJG.ColumnMappings.Add("GenerosId", "GenerosId");
                    bulkCopyJG.ColumnMappings.Add("JogosId", "JogosId");
                    await bulkCopyJG.WriteToServerAsync(tableJogoGenero);
                }
            }

            // 8. Criar Usuários Padrão e Configurar Perfil com Jogos Top-Tier Reais
            var userCount = await _context.Usuarios.CountAsync();
            if (userCount == 0)
            {
                var hashSenha = BCrypt.Net.BCrypt.HashPassword("Password123!");
                var pedro = new Usuario
                {
                    NomeUsuario = "pedro",
                    Email = "pedro@gamelog.com",
                    Senha = hashSenha,
                    Bio = "Gamer apaixonado e colecionador de platinas no GameLog.",
                    FotoDePerfil = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&h=150&fit=crop&crop=faces",
                    EstaAtivo = true
                };
                var ana = new Usuario
                {
                    NomeUsuario = "ana_gamer",
                    Email = "ana@gamelog.com",
                    Senha = hashSenha,
                    Bio = "Explorando mundos fantásticos e jogos narrativos.",
                    FotoDePerfil = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&h=150&fit=crop&crop=faces",
                    EstaAtivo = true
                };
                var lucas = new Usuario
                {
                    NomeUsuario = "lucas_retro",
                    Email = "lucas@gamelog.com",
                    Senha = hashSenha,
                    Bio = "Amante de clássicos dos anos 90 e 2000.",
                    FotoDePerfil = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&h=150&fit=crop&crop=faces",
                    EstaAtivo = true
                };

                _context.Usuarios.AddRange(pedro, ana, lucas);
                await _context.SaveChangesAsync();

                // Estabelecer conexões sociais
                _context.SegueUsuarios.Add(new SegueUsuario { UsuarioSeguidor = pedro, UsuarioSeguido = ana, EstaAtivo = true });
                _context.SegueUsuarios.Add(new SegueUsuario { UsuarioSeguidor = pedro, UsuarioSeguido = lucas, EstaAtivo = true });
                _context.SegueUsuarios.Add(new SegueUsuario { UsuarioSeguidor = ana, UsuarioSeguido = pedro, EstaAtivo = true });
                _context.SegueUsuarios.Add(new SegueUsuario { UsuarioSeguidor = lucas, UsuarioSeguido = pedro, EstaAtivo = true });

                // Pegar obras-primas canônicas lendárias para o pódio de Pedro e feed social
                var titulosDesejados = new[]
                {
                    "The Witcher 3: Wild Hunt",
                    "Red Dead Redemption 2",
                    "The Legend of Zelda: Tears of the Kingdom",
                    "Grand Theft Auto V",
                    "Baldur's Gate III",
                    "Cyberpunk 2077",
                    "Dark Souls III"
                };

                var topJogos = await _context.Jogos
                    .Where(j => titulosDesejados.Contains(j.Titulo))
                    .Take(5)
                    .ToListAsync();

                if (topJogos.Count < 5)
                {
                    var fallback = await _context.Jogos
                        .Where(j => !topJogos.Select(t => t.Id).Contains(j.Id))
                        .Take(5 - topJogos.Count)
                        .ToListAsync();
                    topJogos.AddRange(fallback);
                }

                for (int i = 0; i < topJogos.Count; i++)
                {
                    _context.JogosFavoritosUsuarios.Add(new JogoFavoritoUsuario
                    {
                        UsuarioId = pedro.Id,
                        JogoId = topJogos[i].Id,
                        Posicao = i + 1,
                        EstaAtivo = true
                    });

                    _context.ItensBiblioteca.Add(new BibliotecaJogo
                    {
                        UsuarioId = pedro.Id,
                        JogoId = topJogos[i].Id,
                        Status = i < 3 ? StatusJogo.Zerado : StatusJogo.Jogando,
                        DataAtualizacao = DateTime.UtcNow,
                        EstaAtivo = true
                    });
                }
                await _context.SaveChangesAsync();

                // Criar avaliações ricas e interações para o feed social
                if (topJogos.Count >= 3)
                {
                    var av1 = new Avaliacao
                    {
                        Jogo = topJogos[0],
                        Usuario = ana,
                        Nota = 5,
                        TextoAvaliacao = "Experiência absolutamente fantástica! Trilha sonora impecável e mundo envolvente que prende do início ao fim.",
                        DataPublicacao = DateTime.UtcNow.AddDays(-2),
                        EstaAtivo = true
                    };
                    var av2 = new Avaliacao
                    {
                        Jogo = topJogos[1],
                        Usuario = lucas,
                        Nota = 5,
                        TextoAvaliacao = "Um dos melhores jogos da geração. O design das missões e a liberdade de escolhas são impressionantes.",
                        DataPublicacao = DateTime.UtcNow.AddDays(-1),
                        EstaAtivo = true
                    };
                    var av3 = new Avaliacao
                    {
                        Jogo = topJogos[2],
                        Usuario = pedro,
                        Nota = 5,
                        TextoAvaliacao = "Combate preciso e desafiador na medida certa. Uma obra-prima moderna do entretenimento interativo!",
                        DataPublicacao = DateTime.UtcNow,
                        EstaAtivo = true
                    };
                    _context.Avaliacoes.AddRange(av1, av2, av3);
                    await _context.SaveChangesAsync();

                    _context.CurtidasDeAvaliacoes.Add(new CurtidaDeAvaliacao { AvaliacaoId = av1.Id, UsuarioId = pedro.Id, Curtida = true, EstaAtivo = true });
                    _context.CurtidasDeAvaliacoes.Add(new CurtidaDeAvaliacao { AvaliacaoId = av2.Id, UsuarioId = pedro.Id, Curtida = true, EstaAtivo = true });
                    _context.CurtidasDeAvaliacoes.Add(new CurtidaDeAvaliacao { AvaliacaoId = av3.Id, UsuarioId = ana.Id, Curtida = true, EstaAtivo = true });
                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogInformation("[Seeder] Catálogo semeado com {Total} jogos 100% autênticos e descrições ricas!", tableJogos.Rows.Count);
        }

        private static string NormalizarTituloParaDeduplicacao(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return string.Empty;
            var lower = title.ToLowerInvariant().Trim();

            // Limpar sufixos de edições redundantes para evitar entradas duplicadas do mesmo jogo
            lower = Regex.Replace(lower, @"\(\d{4}\)", "");
            lower = Regex.Replace(lower, @"\b(game of the year edition|goty edition|definitive edition|enhanced edition|remastered|remake|goodies collection|digital deluxe edition|special edition|anniversary edition|legendary edition|complete edition|directors cut|director's cut|hd remaster|deluxe edition|gold edition|collector's edition|collectors edition|anthology|trilogy|compilation|two-pack|double pack|edition)\b", "");

            // Remover caracteres especiais e espaços
            return Regex.Replace(lower, @"[^a-z0-9]", "");
        }

        private static string FormatarTituloFinal(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return title;
            var clean = title.Trim();
            clean = clean.Replace(" & Phantom Liberty Goodies Collection", "")
                         .Replace(" Goodies Collection", "")
                         .Trim();
            return clean;
        }

        private static (List<Guid> Ids, List<string> Nomes) ResolverGeneros(RawgGameItemDTO rawg, Dictionary<string, Guid> generoDict, Guid defaultGeneroId)
        {
            var gensIds = new List<Guid>();
            var gensNomes = new List<string>();

            if (rawg.Genres != null && rawg.Genres.Any())
            {
                foreach (var g in rawg.Genres)
                {
                    var ptName = MapearGeneroInglesParaPortugues(g.Name);
                    if (generoDict.TryGetValue(ptName, out var gId))
                    {
                        gensIds.Add(gId);
                        gensNomes.Add(ptName);
                    }
                }
            }

            // Mapeamento enriquecido baseado no título e slug
            var lower = (rawg.Name + " " + rawg.Slug).ToLowerInvariant();
            if (lower.Contains("souls") || lower.Contains("elden ring") || lower.Contains("bloodborne") || lower.Contains("witcher"))
            {
                if (generoDict.TryGetValue("Fantasia Sombria", out var gId)) { gensIds.Add(gId); gensNomes.Add("Fantasia Sombria"); }
                if (generoDict.TryGetValue("RPG", out var rpgId)) { gensIds.Add(rpgId); gensNomes.Add("RPG"); }
            }
            if (lower.Contains("cyberpunk") || lower.Contains("deus ex"))
            {
                if (generoDict.TryGetValue("Cyberpunk", out var gId)) { gensIds.Add(gId); gensNomes.Add("Cyberpunk"); }
            }
            if (lower.Contains("metroid") || lower.Contains("castlevania") || lower.Contains("hollow knight") || lower.Contains("blasphemous") || lower.Contains("ori and"))
            {
                if (generoDict.TryGetValue("Metroidvania", out var gId)) { gensIds.Add(gId); gensNomes.Add("Metroidvania"); }
            }
            if (lower.Contains("resident evil") || lower.Contains("silent hill") || lower.Contains("dead space") || lower.Contains("outlast") || lower.Contains("amnesia") || lower.Contains("survival horror"))
            {
                if (generoDict.TryGetValue("Terror & Sobrevivência", out var gId)) { gensIds.Add(gId); gensNomes.Add("Terror & Sobrevivência"); }
            }
            if (lower.Contains("rogue") || lower.Contains("hades") || lower.Contains("dead cells") || lower.Contains("binding of isaac") || lower.Contains("slay the spire"))
            {
                if (generoDict.TryGetValue("Roguelike", out var gId)) { gensIds.Add(gId); gensNomes.Add("Roguelike"); }
            }
            if (lower.Contains("devil may cry") || lower.Contains("bayonetta") || lower.Contains("god of war") || lower.Contains("ninja gaiden"))
            {
                if (generoDict.TryGetValue("Hack and Slash", out var gId)) { gensIds.Add(gId); gensNomes.Add("Hack and Slash"); }
            }
            if (lower.Contains("gta") || lower.Contains("grand theft auto") || lower.Contains("red dead") || lower.Contains("assassin's creed") || lower.Contains("horizon") || lower.Contains("spider-man") || lower.Contains("zelda"))
            {
                if (generoDict.TryGetValue("Mundo Aberto", out var gId)) { gensIds.Add(gId); gensNomes.Add("Mundo Aberto"); }
            }
            if (lower.Contains("metal gear") || lower.Contains("hitman") || lower.Contains("splinter cell") || lower.Contains("dishonored") || lower.Contains("thief"))
            {
                if (generoDict.TryGetValue("Stealth", out var gId)) { gensIds.Add(gId); gensNomes.Add("Stealth"); }
            }

            var distinctIds = gensIds.Distinct().ToList();
            var distinctNomes = gensNomes.Distinct().ToList();

            if (!distinctIds.Any())
            {
                distinctIds.Add(defaultGeneroId);
                distinctNomes.Add("Ação");
            }

            return (distinctIds, distinctNomes);
        }

        private static string GerarDescricaoRica(string titulo, RawgGameItemDTO rawg, string studioNome, List<string> generosNomes, DateTime dtLanc)
        {
            var lower = (titulo + " " + (rawg.Slug ?? "")).ToLowerInvariant();

            // 1. Sinopses Culturais e Históricas Consagradas para as 100+ Grandes Franquias
            if (lower.Contains("witcher"))
                return "Em um continente devastado pela guerra e assolado por monstros lendários, o caçador geneticamente aprimorado Geralt de Rívia busca a Criança da Profecia. O título entrega uma narrativa ramificada magistral, contratos de caça complexos e exploração rica em um universo de fantasia sombria inesquecível.";

            if (lower.Contains("elden ring"))
                return "Nas Terras Intermédias regidas pela Rainha Marika, o Maculado é guiado pela graça para restaurar o Anel Prístino e ascender como Lorde Prístino. Combina o combate tático desafiador característico da FromSoftware com um colossal mundo aberto pontuado por masmorras secretas e semideuses lendários.";

            if (lower.Contains("grand theft auto") || lower.Contains("gta"))
                return "Uma obra-prima do entretenimento interativo que explora a ambição e o submundo do crime em megalópoles vibrantes e implacáveis. Apresenta liberdade absoluta de exploração em mundo aberto, assaltos espetaculares, física veicular refinada e uma sátira mordaz da sociedade contemporânea.";

            if (lower.Contains("red dead"))
                return "Um épico faroeste cinematográfico que retrata o declínio dos foras da lei na fronteira americana. O jogo oferece uma imersão incomparável no Velho Oeste, com tiroteios intensos, caça, sobrevivência na natureza e uma profunda reflexão sobre redenção e lealdade.";

            if (lower.Contains("zelda"))
                return "Uma lendária jornada de fantasia e heroísmo onde Link desbrava os segredos milenares do reino de Hyrule. O jogador resolve quebra-cabeças em templos ancestrais, domina habilidades mágicas e empunha a Master Sword para proteger o reino e a Princesa Zelda.";

            if (lower.Contains("god of war"))
                return "Uma saga mitológica visceral que acompanha Kratos em sua brutal cruzada contra os deuses do Olimpo e, posteriormente, em sua jornada de redenção e paternidade pelas terras nórdicas ao lado de seu filho Atreus.";

            if (lower.Contains("cyberpunk 2077") || lower.Contains("cyberpunk"))
                return "Uma aventura eletrizante de RPG e ação pelas ruas iluminadas por neon de Night City, uma megalópole futurista dominada por megacorporações e implantes cibernéticos. O jogador forja seu próprio destino como mercenário urbano em busca da imortalidade.";

            if (lower.Contains("baldurs gate") || lower.Contains("baldur's gate"))
                return "Um RPG monumental baseado nas regras de Dungeons & Dragons, onde cada escolha moral, diálogo e rolagem de dados molda o destino de Faerûn. Conta com combate tático refinado por turnos, companheiros memoráveis e liberdade incomparável de abordagem.";

            if (lower.Contains("resident evil"))
                return "O ápice do survival horror que combina atmosfera aterrorizante, gerenciamento estratégico de recursos e combate claustrofóbico contra abominações biológicas e conspirações corporativas em cenários detalhados e cheios de enigmas.";

            if (lower.Contains("silent hill"))
                return "Uma profunda imersão no terror psicológico onde ruas nebulosas e o sombrio Outromundo refletem os medos e traumas da mente humana. Oferece enigmas instigantes, trilha sonora atmosférica e narrativa perturbadora.";

            if (lower.Contains("the last of us"))
                return "Uma jornada humana devastadora e emocionante através de uma América pós-apocalíptica tomada por fungos parasitas e facções implacáveis, focando na sobrevivência, no sacrifício e nos laços inquebráveis entre seus protagonistas.";

            if (lower.Contains("uncharted"))
                return "Uma espetacular aventura de ação cinematográfica onde o carismático caçador de tesouros Nathan Drake viaja pelos cantos mais perigosos do planeta em busca de cidades perdidas e relíquias históricas lendárias.";

            if (lower.Contains("tomb raider"))
                return "A lendária arqueóloga Lara Croft desbrava tumbas ancestrais, florestas perigosas e ruínas misteriosas ao redor do globo, superando quebra-cabeças letais, tiroteios intensos e conspirações milenares.";

            if (lower.Contains("ghost of tsushima"))
                return "No Japão feudal do século XIII, o samurai Jin Sakai desafia o código de honra tradicional para se transformar no temido Fantasma e libertar a Ilha de Tsushima da brutal invasão do Império Mongol.";

            if (lower.Contains("spider-man"))
                return "O herói aracnídeo balança com teias fluidas pelos arranha-céus de Nova York, enfrentando uma galeria icônica de supervilões enquanto equilibra sua vida pessoal turbulenta com o peso da grande responsabilidade.";

            if (lower.Contains("batman") || lower.Contains("arkham"))
                return "O Cavaleiro das Trevas patrulha as sombras de Gotham City utilizando combate corpo a corpo fluido FreeFlow, aparelhos de alta tecnologia e raciocínio de detetive para derrotar seus piores arqui-inimigos.";

            if (lower.Contains("yakuza") || lower.Contains("like a dragon") || lower.Contains("judgment"))
                return "Mergulha no submundo da máfia japonesa através das ruas movimentadas de Kamurocho, combinando combates brutais de pancadaria, enredo dramático sobre honra e lealdade, e uma infinidade de minijogos e histórias secundárias hilárias.";

            if (lower.Contains("pokemon") || lower.Contains("pokémon"))
                return "Uma adorada aventura de exploração e treinamento de criaturas místicas por diversas regiões fantásticas, onde jovens treinadores batalham em ginásios, completam sua Pokédex e enfrentam a Elite dos Quatro para se tornarem Mestres Pokémon.";

            if (lower.Contains("metroid"))
                return "A lendária caçadora de recompensas espacial Samus Aran explora planetas alienígenas hostis e labirínticos, desbloqueando armas de feixe de energia, o Morph Ball e armaduras futuristas para erradicar ameaças biológicas cósmicas.";

            if (lower.Contains("mega man"))
                return "O clássico herói robótico azul enfrenta legiões de Robot Masters criados pelo Dr. Wily, combinando plataforma de precisão milimétrica e a mecânica icônica de absorver as armas dos chefes derrotados.";

            if (lower.Contains("dragon quest"))
                return "O clássico supremo do JRPG tradicional com arte inconfundível de Akira Toriyama, onde o Herói escolhido reúne aliados valorosos em uma épica fantasia medieval para banir as forças das trevas.";

            if (lower.Contains("kingdom hearts"))
                return "Uma emocionante fusão entre os universos da Square Enix e da Disney, onde Sora empunha a lendária Keyblade ao lado de Donald e Pateta para combater os Heartless e proteger a luz dos mundos.";

            if (lower.Contains("nier"))
                return "Uma obra-prima filosófica e melancólica que explora a consciência, a humanidade e o existencialismo através de combates frenéticos, trilha sonora arrebatadora e múltiplos finais reveladores.";

            if (lower.Contains("persona") || lower.Contains("shin megami tensei"))
                return "Uma aclamada combinação de RPG clássico por turnos com simulação de vida e relacionamentos no Japão moderno. Os protagonistas despertam o poder de suas Personas interiores para combater a corrupção psicológica da sociedade.";

            if (lower.Contains("dark souls") || lower.Contains("demon's souls") || lower.Contains("bloodborne") || lower.Contains("sekiro"))
                return "Um RPG sombrio e implacável marcado por uma atmosfera melancólica, arquitetura gótica imponente, mitologia profunda e combates milimétricos que desafiam os reflexos e a perseverança do jogador contra horrores épicos e cavaleiros corrompidos.";

            if (lower.Contains("final fantasy"))
                return "Um épico espetacular de RPG que transporta os jogadores para reinos deslumbrantes com trilha sonora orquestrada memorável, personagens icônicos, invocações colossais e um combate dinâmico que equilibra estratégia e ação cinematográfica.";

            if (lower.Contains("hollow knight"))
                return "Uma aclamada aventura metroidvania desenhada à mão através das ruínas esquecidas do reino de Hallownest. O jogador domina combates ágeis com seu ferrão, aprende magias antigas e desvenda segredos enterrados em labirintos subterrâneos interconectados.";

            if (lower.Contains("hades"))
                return "Um roguelike de ação frenética onde Zagreu, o príncipe do Submundo, luta para escapar dos domínios de seu pai Hades. Cada tentativa de fuga oferece novas combinações de bênçãos olímpicas, diálogos dublados dinâmicos e combates fluidos e gratificantes.";

            if (lower.Contains("black myth"))
                return "Um impressionante RPG de ação fundamentado na mitologia clássica de 'Jornada ao Oeste'. Como o Predestinado, o jogador empunha o lendário bastão mágico, aprende feitiços arcanos e transformações místicas para enfrentar deuses e feras mitológicas colossais.";

            if (lower.Contains("helldivers"))
                return "Um shooter cooperativo frenético em terceira pessoa onde soldados de elite lutam pela galáxia para espalhar a Democracia Gerenciada. Esquadrões de 4 jogadores utilizam artilharia pesada, ataques aéreos e armas táticas contra invasões alienígenas maciças.";

            if (lower.Contains("palworld"))
                return "Uma vibrante aventura de sobrevivência, criação e mundo aberto onde os jogadores exploram terras desconhecidas, capturam criaturas misteriosas com habilidades únicas, constroem bases industriais e enfrentam chefes e caçadores ilegais.";

            if (lower.Contains("half-life"))
                return "Um marco revolucionário na história dos jogos de tiro em primeira pessoa, introduzindo narrativa ambiental sem cortes cinematográficos, inteligência artificial avançada e quebra-cabeças com física que redefiniram a indústria dos videogames.";

            if (lower.Contains("portal"))
                return "Um brilhante jogo de quebra-cabeças em primeira pessoa ambientado nos laboratórios da Aperture Science, onde o jogador manipula a física e a perspectiva espacial usando a icônica arma de portais sob os olhares sarcásticos da IA GLaDOS.";

            if (lower.Contains("chrono trigger") || lower.Contains("chrono cross"))
                return "Um dos maiores RPGs de todos os tempos, unindo os talentos de Hironobu Sakaguchi, Yuji Horii e Akira Toriyama em uma inesquecível aventura através das eras temporais para salvar o futuro do planeta contra a destruição de Lavos.";

            if (lower.Contains("castlevania"))
                return "Uma aventura gótica clássica nas câmaras misteriosas do Castelo do Drácula, repleta de passagens secretas, criaturas da noite, chicotes sagrados, magias ancestrais e labirintos fascinantes.";

            if (lower.Contains("street fighter") || lower.Contains("tekken") || lower.Contains("mortal kombat") || lower.Contains("king of fighters") || lower.Contains("guilty gear") || lower.Contains("soulcalibur"))
                return "Combates competitivos de alta precisão com um elenco lendário de lutadores, cada um com golpes especiais devastadores, artes marciais distintas e combos espetaculares em arenas globais.";

            if (lower.Contains("mario") || lower.Contains("donkey kong") || lower.Contains("kirby") || lower.Contains("banjo"))
                return "A essência pura dos jogos de plataforma com controles impecáveis, design de fases engenhoso e mundos coloridos repletos de segredos, moedas e transformações divertidas em uma jornada alegre e desafiadora.";

            if (lower.Contains("sonic"))
                return "Acelere em alta velocidade com o lendário Ouriço Azul por pistas espetaculares com loopings vertiginosos, coletando argolas douradas e frustrando os planos do cientista vilão Dr. Eggman.";

            if (lower.Contains("crash bandicoot") || lower.Contains("spyro"))
                return "Clássicos memoráveis dos mascotes de plataforma 3D, com saltos de alta precisão, perseguições eletrizantes, mundos mágicos e desafios divertidos para coletar todas as relíquias e gemas.";

            if (lower.Contains("fallout"))
                return "Um RPG pós-apocalíptico marcante ambientado em uma América retrofuturista devastada por guerra nuclear, oferecendo escolhas morais complexas, exploração de refúgios subterrâneos e terras ermas radioativas.";

            if (lower.Contains("skyrim") || lower.Contains("elder scrolls") || lower.Contains("oblivion") || lower.Contains("morrowind"))
                return "Um épico de RPG de mundo aberto onde o jogador assume o papel do herói destinado a explorar as províncias ricas de Tamriel, dominando feitiços arcanos, forjando armas lendárias e decidindo o destino de reinos inteiros.";

            if (lower.Contains("doom") || lower.Contains("quake") || lower.Contains("wolfenstein"))
                return "O ápice do combate visceral em primeira pessoa, onde o guerreiro enfrenta hordas implacáveis de inimigos mortais com um arsenal devastador de armas de fogo, velocidade implacável e ação ininterrupta.";

            if (lower.Contains("call of duty") || lower.Contains("battlefield") || lower.Contains("medal of honor"))
                return "A experiência definitiva de tiro militar em grande escala, entregando combates intensos de infantaria, veículos de combate, operações táticas de alta adrenalina e cenários com destruição dinâmica.";

            if (lower.Contains("halo"))
                return "Uma consagrada ópera espacial militar onde o lendário supersoldado Master Chief e a IA Cortana lutam contra a aliança alienígena Covenant e parasitas ancestrais pelo destino da humanidade.";

            if (lower.Contains("gears of war"))
                return "Tiro tático visceral em terceira pessoa focado no sistema de cobertura, onde os soldados da Coalizão enfrentam a brutal invasão dos Locust do subsolo com rifles com motosserras e combates ferozes.";

            if (lower.Contains("mass effect"))
                return "Uma consagrada ópera espacial onde o Comandante Shepard lidera a tripulação da Normandy para salvar a galáxia da ameaça iminente dos Ceifadores, com escolhas morais profundas e relacionamentos inesquecíveis.";

            if (lower.Contains("dragon age"))
                return "Um épico de fantasia sombria e RPG tático onde heróis improváveis formam alianças políticas, enfrentam a Corrupção dos Darkspawns e navegam por intrigas no continente místico de Thedas.";

            if (lower.Contains("bioshock"))
                return "Uma jornada inesquecível por cidades utópicas que caíram em desgraça filosófica, misturando tiroteios com poderes genéticos de Plasmídeos e uma das reviravoltas narrativas mais famosas da história dos games.";

            if (lower.Contains("dead space"))
                return "O engenheiro Isaac Clarke enfrenta horrores cósmicos e abominações necromorfas a bordo de espaçonaves de mineração abandonadas, utilizando ferramentas industriais para desmembrar inimigos estrategicamente.";

            if (lower.Contains("assassin's creed") || lower.Contains("assassins creed"))
                return "Uma saga histórica épica que mergulha em guerras secretas entre Assassinos e Templários através de períodos históricos fascinantes, com movimentação fluida de parkour, assassinatos furtivos e cenários ricamente detalhados.";

            if (lower.Contains("far cry"))
                return "Uma explosiva experiência de tiro em primeira pessoa em mundos abertos exóticos governados por líderes tirânicos carismáticos, oferecendo combates caóticos, animais selvagens imprevisíveis e destruição criativa.";

            if (lower.Contains("hitman") || lower.Contains("splinter cell"))
                return "O ápice da furtividade e infiltração tática profissional, onde agentes de elite executam missões secretas pelo mundo com disfarces, planejamento cirúrgico e múltiplas formas criativas de atingir seus alvos.";

            if (lower.Contains("dishonored") || lower.Contains("deus ex") || lower.Contains("prey"))
                return "Um refinado simulador imersivo onde habilidades sobrenaturais ou implantes cibernéticos permitem resolver missões por rotas furtivas, combates diretos ou manipulação inteligente do ambiente.";

            if (lower.Contains("monster hunter"))
                return "A caçada definitiva em ecossistemas exuberantes e dinâmicos, onde caçadores enfrentam monstros gigantescos, dominam armas complexas e forjam equipamentos poderosos a partir de recursos obtidos na natureza.";

            if (lower.Contains("devil may cry") || lower.Contains("bayonetta") || lower.Contains("ninja gaiden"))
                return "O pináculo da ação estilosa e combate hack-and-slash acrobático, permitindo encadear combos espetaculares com espadas e armas de fogo para obter as notas mais altas de estilo e técnica.";

            if (lower.Contains("need for speed") || lower.Contains("burnout") || lower.Contains("midnight club"))
                return "A adrenalina pura do automobilismo de rua e corridas clandestinas ilegais, com customização estética e mecânica profunda de superesportivos e perseguições policiais frenéticas.";

            if (lower.Contains("forza") || lower.Contains("gran turismo"))
                return "A simulação automobilística definitiva que reproduz com fidelidade milimétrica o comportamento físico, o som dos motores e o design de centenas dos veículos mais icônicos do mundo em pistas reais e circuitos deslumbrantes.";

            if (lower.Contains("civilization") || lower.Contains("age of empires") || lower.Contains("total war") || lower.Contains("warcraft") || lower.Contains("starcraft"))
                return "Obras magnas da estratégia e liderança onde você constrói impérios através das eras da humanidade, pesquisando tecnologias, gerenciando recursos e comandando exércitos colossais para conquistar a vitória.";

            if (lower.Contains("the sims") || lower.Contains("simcity") || lower.Contains("cities: skylines"))
                return "O clássico supremo da simulação e criatividade, permitindo projetar lares espetaculares, guiar a vida de personagens únicos ou administrar metrópoles gigantescas com transporte, energia e economia dinâmica.";

            if (lower.Contains("minecraft") || lower.Contains("terraria"))
                return "O fenômeno da criatividade e sobrevivência em mundos infinitos gerados proceduralmente, permitindo construir desde cabanas simples até civilizações complexas, minerar recursos raros e sobreviver a noites perigosas.";

            if (lower.Contains("stardew valley") || lower.Contains("animal crossing"))
                return "Um charmoso e relaxante simulador de vida comunitária e campo, onde você cultiva plantações, decora sua vila ou fazenda, faz amizades calorosas e descobre segredos pacíficos no seu próprio ritmo.";

            if (lower.Contains("s.t.a.l.k.e.r.") || lower.Contains("stalker") || lower.Contains("metro"))
                return "Uma imersão brutal e atmosférica em zonas de exclusão e túneis subterrâneos pós-apocalípticos, enfrentando anomalias energéticas mortais, facções armadas hostis e mutantes radioativos pela sobrevivência.";

            if (lower.Contains("alan wake") || lower.Contains("control"))
                return "Um suspense cinematográfico que explora mistérios paranormais e forças extradimensionais, combinando combates de alta tensão com manipulação de luz e poderes telecinéticos em uma narrativa envolvente.";

            if (lower.Contains("diablo"))
                return "O clássico definitivo do RPG de ação e hack-and-slash isométrico, onde heróis descem às profundezas do Inferno para banir o Senhor do Terror, coletando pilhas de itens lendários e aprimorando habilidades devastadoras.";

            // 2. Gerador Dinâmico Narrativo Rico baseado em Gêneros, Mecânicas e Estúdio
            var generosStr = generosNomes != null && generosNomes.Any() ? string.Join(", ", generosNomes) : "Ação e Aventura";
            var generoPrincipal = generosNomes?.FirstOrDefault() ?? "Ação";

            var abertura = generoPrincipal switch
            {
                "RPG" => $"Em {titulo}, os jogadores mergulham em uma grandiosa jornada de RPG desenvolvida por {studioNome}.",
                "Ação" => $"{titulo} entrega uma eletrizante experiência de ação intensa criada por {studioNome}.",
                "Aventura" => $"{titulo} convida os jogadores a desbravar um mundo rico em mistérios e exploração concebido por {studioNome}.",
                "Tiro" => $"{titulo} apresenta intensos confrontos armados e combates táticos dinâmicos desenvolvidos por {studioNome}.",
                "Terror & Sobrevivência" => $"{titulo} transporta os jogadores para uma atmosfera tensa e aterrorizante de sobrevivência produzida por {studioNome}.",
                "Estratégia" => $"{titulo} desafia a mente tática dos jogadores em um rico cenário de gerenciamento e estratégia criado por {studioNome}.",
                "Plataforma" => $"{titulo} oferece uma envolvente aventura de plataforma com controles precisos e fases criativas desenhadas por {studioNome}.",
                "Corrida" => $"{titulo} traz toda a adrenalina da alta velocidade e automobilismo competitivo desenvolvida por {studioNome}.",
                "Luta" => $"{titulo} coloca frente a frente lutadores habilidosos em disputas intensas de artes marciais produzidas por {studioNome}.",
                _ => $"Em {titulo}, os jogadores vivenciam uma marcante jornada de {generosStr} desenvolvida por {studioNome}."
            };

            var corpo = $"O título destaca-se pela combinação refinada de exploração imersiva, mecânicas envolventes de jogabilidade ({generosStr}) e desafios que testam a destreza e o raciocínio dos jogadores em cada etapa.";
            var encerramento = $"Lançado oficialmente em {dtLanc.Year}, a obra consolidou seu espaço na comunidade gamer por sua direção artística marcante e apelo duradouro.";

            return $"{abertura} {corpo} {encerramento}";
        }

        public static string ResolverEstudio(RawgGameItemDTO rawg)
        {
            if (rawg.Publishers != null && rawg.Publishers.Any())
            {
                var pub = rawg.Publishers.First().Name;
                if (!string.IsNullOrWhiteSpace(pub) && pub.Trim().Length > 1) return pub.Trim();
            }
            if (rawg.Developers != null && rawg.Developers.Any())
            {
                var dev = rawg.Developers.First().Name;
                if (!string.IsNullOrWhiteSpace(dev) && dev.Trim().Length > 1) return dev.Trim();
            }

            var name = (rawg.Name ?? "").Trim();
            var slug = (rawg.Slug ?? "").Trim();
            var lower = (name + " " + slug).ToLowerInvariant();

            if (lower.Contains("grand theft auto") || lower.Contains("gta") || lower.Contains("red dead") || lower.Contains("bully") || lower.Contains("max payne 3") || lower.Contains("midnight club") || lower.Contains("manhunt") || lower.Contains("the warriors") || lower.Contains("l.a. noire") || lower.Contains("la noire"))
                return "Rockstar Games";

            if (lower.Contains("elden ring") || lower.Contains("dark souls") || lower.Contains("bloodborne") || lower.Contains("sekiro") || lower.Contains("armored core") || lower.Contains("demon's souls") || lower.Contains("king's field") || lower.Contains("tenchu"))
                return "FromSoftware";

            if (lower.Contains("zelda") || lower.Contains("mario") || lower.Contains("pokemon") || lower.Contains("pokémon") || lower.Contains("metroid") || lower.Contains("kirby") || lower.Contains("fire emblem") || lower.Contains("xenoblade") || lower.Contains("smash bros") || lower.Contains("splatoon") || lower.Contains("animal crossing") || lower.Contains("donkey kong") || lower.Contains("pikmin") || lower.Contains("luigi's mansion") || lower.Contains("paper mario") || lower.Contains("wario") || lower.Contains("golden sun") || lower.Contains("earthbound") || lower.Contains("star fox") || lower.Contains("f-zero") || lower.Contains("bayonetta 2") || lower.Contains("bayonetta 3"))
                return "Nintendo";

            if (lower.Contains("the witcher") || lower.Contains("witcher") || lower.Contains("cyberpunk 2077") || lower.Contains("gwent") || lower.Contains("thronebreaker"))
                return "CD Projekt Red";

            if (lower.Contains("va-11 hall-a") || lower.Contains("va11 halla") || lower.Contains("sukeban"))
                return "Sukeban Games / Ysbryd Games";

            if (lower.Contains("baldurs gate") || lower.Contains("baldur's gate") || lower.Contains("divinity: original sin") || lower.Contains("divinity"))
                return "Larian Studios";

            if (lower.Contains("resident evil") || lower.Contains("monster hunter") || lower.Contains("devil may cry") || lower.Contains("street fighter") || lower.Contains("mega man") || lower.Contains("dragons dogma") || lower.Contains("dragon's dogma") || lower.Contains("ace attorney") || lower.Contains("dead rising") || lower.Contains("okami") || lower.Contains("onimusha") || lower.Contains("marvel vs") || lower.Contains("ghosts 'n goblins") || lower.Contains("viewtiful joe") || lower.Contains("strider") || lower.Contains("lost planet") || lower.Contains("dino crisis") || lower.Contains("exoprimal") || lower.Contains("kunitsu-gami") || lower.Contains("sengoku basara") || lower.Contains("breath of fire") || lower.Contains("asura's wrath"))
                return "Capcom";

            if (lower.Contains("final fantasy") || lower.Contains("kingdom hearts") || lower.Contains("dragon quest") || lower.Contains("nier") || lower.Contains("chrono trigger") || lower.Contains("chrono cross") || lower.Contains("octopath") || lower.Contains("bravely default") || lower.Contains("star ocean") || lower.Contains("tomb raider") || lower.Contains("deus ex") || lower.Contains("life is strange") || lower.Contains("just cause") || lower.Contains("thief") || lower.Contains("legacy of kain") || lower.Contains("front mission") || lower.Contains("parasite eve") || lower.Contains("valkyrie profile") || lower.Contains("mana") || lower.Contains("foamstars") || lower.Contains("outriders") || lower.Contains("sleeping dogs") || lower.Contains("the world ends with you") || lower.Contains("tactics ogre") || lower.Contains("triangle strategy") || lower.Contains("forspoken") || lower.Contains("guardians of the galaxy"))
                return "Square Enix";

            if (lower.Contains("god of war") || lower.Contains("the last of us") || lower.Contains("uncharted") || lower.Contains("ghost of tsushima") || lower.Contains("horizon zero dawn") || lower.Contains("horizon forbidden") || lower.Contains("horizon call of the mountain") || lower.Contains("gran turismo") || lower.Contains("ratchet & clank") || lower.Contains("infamous") || lower.Contains("killzone") || lower.Contains("returnal") || lower.Contains("days gone") || lower.Contains("astro bot") || lower.Contains("shadow of the colossus") || lower.Contains("the last guardian") || lower.Contains("littlebigplanet") || lower.Contains("sackboy") || lower.Contains("resistance") || lower.Contains("gravity rush") || lower.Contains("wipeout") || lower.Contains("siren") || lower.Contains("twisted metal") || lower.Contains("ape escape") || lower.Contains("sly cooper") || lower.Contains("jak and daxter") || lower.Contains("marvel's spider-man") || lower.Contains("spider-man") || lower.Contains("wolverine") || lower.Contains("until dawn") || lower.Contains("the order: 1886") || lower.Contains("motorstorm") || lower.Contains("medievil"))
                return "PlayStation Studios";

            if (lower.Contains("halo") || lower.Contains("gears of war") || lower.Contains("gears 5") || lower.Contains("forza") || lower.Contains("fable") || lower.Contains("sea of thieves") || lower.Contains("age of empires") || lower.Contains("state of decay") || lower.Contains("psychonauts") || lower.Contains("minecraft") || lower.Contains("avowed") || lower.Contains("senua's saga") || lower.Contains("hellblade") || lower.Contains("banjo") || lower.Contains("conker") || lower.Contains("perfect dark") || lower.Contains("killer instinct") || lower.Contains("battletoads") || lower.Contains("viva piñata") || lower.Contains("grounded") || lower.Contains("pentiment") || lower.Contains("flight simulator") || lower.Contains("crackdown") || lower.Contains("wasteland") || lower.Contains("the outer worlds") || lower.Contains("fallout: new vegas") || lower.Contains("pillars of eternity") || lower.Contains("clockwork revolution") || lower.Contains("south of midnight"))
                return "Xbox Game Studios";

            if (lower.Contains("assassins creed") || lower.Contains("assassin's creed") || lower.Contains("far cry") || lower.Contains("rainbow six") || lower.Contains("watch dogs") || lower.Contains("splinter cell") || lower.Contains("ghost recon") || lower.Contains("prince of persia") || lower.Contains("rayman") || lower.Contains("the division") || lower.Contains("avatar: frontiers") || lower.Contains("skull and bones") || lower.Contains("the crew") || lower.Contains("beyond good & evil") || lower.Contains("for honor") || lower.Contains("driver: san francisco") || lower.Contains("driver 76") || lower.Contains("driver: parallel") || lower.Contains("trackmania") || lower.Contains("rabbids") || lower.Contains("child of light") || lower.Contains("valiant hearts") || lower.Contains("immortals fenyx") || lower.Contains("south park") || lower.Contains("star wars outlaws") || lower.Contains("brawlhalla") || lower.Contains("steep") || lower.Contains("riders republic"))
                return "Ubisoft";

            if (lower.Contains("fifa") || lower.Contains("ea sports") || lower.Contains("battlefield") || lower.Contains("need for speed") || lower.Contains("the sims") || lower.Contains("mass effect") || lower.Contains("dragon age") || lower.Contains("dead space") || lower.Contains("apex legends") || lower.Contains("titanfall") || lower.Contains("star wars jedi") || lower.Contains("battlefront") || lower.Contains("star wars: squadrons") || lower.Contains("star wars: the old republic") || lower.Contains("it takes two") || lower.Contains("command & conquer") || lower.Contains("generals") || lower.Contains("red alert") || lower.Contains("tiberium") || lower.Contains("simcity") || lower.Contains("spore") || lower.Contains("burnout") || lower.Contains("mirror's edge") || lower.Contains("skate") || lower.Contains("medal of honor") || lower.Contains("plants vs. zombies") || lower.Contains("unravel") || lower.Contains("a way out") || lower.Contains("anthem") || lower.Contains("wild hearts") || lower.Contains("immortals of aveum") || lower.Contains("alice: madness"))
                return "Electronic Arts";

            if (lower.Contains("call of duty") || lower.Contains("warcraft") || lower.Contains("diablo") || lower.Contains("starcraft") || lower.Contains("overwatch") || lower.Contains("crash bandicoot") || lower.Contains("spyro") || lower.Contains("tony hawk") || lower.Contains("hearthstone") || lower.Contains("guitar hero") || lower.Contains("prototype") || lower.Contains("singularity") || lower.Contains("heroes of the storm"))
                return "Activision Blizzard";

            if (lower.Contains("persona") || lower.Contains("shin megami tensei") || lower.Contains("metaphor: refantazio") || lower.Contains("catherine") || lower.Contains("soul hackers") || lower.Contains("etrian odyssey"))
                return "Atlus";

            if (lower.Contains("yakuza") || lower.Contains("like a dragon") || lower.Contains("sonic") || lower.Contains("total war") || lower.Contains("judgment") || lower.Contains("valkyria chronicles") || lower.Contains("super monkey ball") || lower.Contains("football manager") || lower.Contains("shenmue") || lower.Contains("virtua fighter") || lower.Contains("crazy taxi") || lower.Contains("jet set radio") || lower.Contains("phantasy star") || lower.Contains("bayonetta") || lower.Contains("golden axe") || lower.Contains("streets of rage") || lower.Contains("shinobi") || lower.Contains("house of the dead") || lower.Contains("puyo puyo") || lower.Contains("alien: isolation") || lower.Contains("aliens vs. predator") || lower.Contains("company of heroes") || lower.Contains("two point") || lower.Contains("endless space") || lower.Contains("endless legend"))
                return "Sega";

            if (lower.Contains("tekken") || lower.Contains("tales of") || lower.Contains("pac-man") || lower.Contains("soulcalibur") || lower.Contains("dragon ball") || lower.Contains("naruto") || lower.Contains("one piece") || lower.Contains("gundam") || lower.Contains("scarlet nexus") || lower.Contains("ace combat") || lower.Contains("little nightmares") || lower.Contains("code vein") || lower.Contains("digimon") || lower.Contains("ni no kuni") || lower.Contains("god eater") || lower.Contains("katamari") || lower.Contains("sand land") || lower.Contains("dark pictures") || lower.Contains("man of medan") || lower.Contains("little hope") || lower.Contains("house of ashes") || lower.Contains("devil in me") || lower.Contains("frank stone") || lower.Contains("jujutsu kaisen") || lower.Contains("bleach") || lower.Contains("my hero") || lower.Contains("sword art online"))
                return "Bandai Namco";

            if (lower.Contains("half-life") || lower.Contains("portal") || lower.Contains("left 4 dead") || lower.Contains("counter-strike") || lower.Contains("team fortress") || lower.Contains("dota 2") || lower.Contains("day of defeat") || lower.Contains("ricochet") || lower.Contains("artifact") || lower.Contains("underlords"))
                return "Valve Corporation";

            if (lower.Contains("skyrim") || lower.Contains("elder scrolls") || lower.Contains("fallout") || lower.Contains("starfield") || lower.Contains("dishonored") || lower.Contains("deathloop") || lower.Contains("doom") || lower.Contains("wolfenstein") || lower.Contains("prey") || lower.Contains("quake") || lower.Contains("the evil within") || lower.Contains("rage") || lower.Contains("ghostwire") || lower.Contains("hi-fi rush") || lower.Contains("heretic") || lower.Contains("hexen") || lower.Contains("redfall"))
                return "Bethesda Softworks";

            if (lower.Contains("alan wake") || lower.Contains("control") || lower.Contains("quantum break") || lower.Contains("max payne"))
                return "Remedy Entertainment";

            if (lower.Contains("mortal kombat") || lower.Contains("injustice") || lower.Contains("batman: arkham") || lower.Contains("arkham") || lower.Contains("hogwarts legacy") || lower.Contains("suicide squad") || lower.Contains("gotham knights") || lower.Contains("middle-earth") || lower.Contains("shadow of mordor") || lower.Contains("shadow of war") || lower.Contains("lego") || lower.Contains("mad max") || lower.Contains("f.e.a.r.") || lower.Contains("fear 2") || lower.Contains("fear 3") || lower.Contains("gauntlet") || lower.Contains("scribblenauts") || lower.Contains("multiversus") || lower.Contains("back 4 blood") || lower.Contains("hitman 2") || lower.Contains("hitman: contracts") || lower.Contains("hitman: blood money"))
                return "Warner Bros. Games";

            if (lower.Contains("borderlands") || lower.Contains("bioshock") || lower.Contains("civilization") || lower.Contains("mafia") || lower.Contains("nba 2k") || lower.Contains("wwe 2k") || lower.Contains("xcom") || lower.Contains("tiny tina") || lower.Contains("the darkness") || lower.Contains("spec ops") || lower.Contains("duke nukem") || lower.Contains("midnight suns") || lower.Contains("the quarry"))
                return "2K Games";

            if (lower.Contains("silent hill") || lower.Contains("castlevania") || lower.Contains("metal gear") || lower.Contains("efootball") || lower.Contains("pes") || lower.Contains("yu-gi-oh") || lower.Contains("contra") || lower.Contains("bomberman") || lower.Contains("gradius") || lower.Contains("zone of the enders") || lower.Contains("suikoden"))
                return "Konami";

            if (lower.Contains("metro 2033") || lower.Contains("metro last light") || lower.Contains("metro exodus") || lower.Contains("metro") || lower.Contains("dead island") || lower.Contains("saints row") || lower.Contains("darksiders") || lower.Contains("biomutant") || lower.Contains("kingdoms of amalur") || lower.Contains("destroy all humans") || lower.Contains("elex") || lower.Contains("outcast") || lower.Contains("remnant") || lower.Contains("homefront") || lower.Contains("titan quest") || lower.Contains("alone in the dark"))
                return "Deep Silver / THQ Nordic";

            if (lower.Contains("a plague tale") || lower.Contains("space marine") || lower.Contains("warhammer 40,000") || lower.Contains("warhammer 40k") || lower.Contains("insurgency") || lower.Contains("aliens: dark descent") || lower.Contains("evil west") || lower.Contains("vampyr") || lower.Contains("the surge") || lower.Contains("snowrunner") || lower.Contains("mudrunner") || lower.Contains("expeditions") || lower.Contains("banishers") || lower.Contains("atomic heart") || lower.Contains("greedfall"))
                return "Focus Entertainment";

            if (lower.Contains("ghostrunner") || lower.Contains("assetto corsa") || lower.Contains("bloodstained") || lower.Contains("abzû") || lower.Contains("abzu") || lower.Contains("payday 2") || lower.Contains("brothers: a tale of two sons"))
                return "505 Games";

            if (lower.Contains("crusader kings") || lower.Contains("hearts of iron") || lower.Contains("europa universalis") || lower.Contains("stellaris") || lower.Contains("cities: skylines") || lower.Contains("victoria") || lower.Contains("age of wonders") || lower.Contains("prison architect") || lower.Contains("surviving mars"))
                return "Paradox Interactive";

            if (lower.Contains("nioh") || lower.Contains("dynasty warriors") || lower.Contains("samurai warriors") || lower.Contains("ninja gaiden") || lower.Contains("dead or alive") || lower.Contains("fatal frame") || lower.Contains("atelier") || lower.Contains("wo long") || lower.Contains("rise of the ronin") || lower.Contains("wild hearts"))
                return "Koei Tecmo";

            if (lower.Contains("danganronpa") || lower.Contains("zero escape") || lower.Contains("ai: the somnium files") || lower.Contains("sparking! zero") || lower.Contains("steins;gate") || lower.Contains("master detective archives"))
                return "Spike Chunsoft";

            if (lower.Contains("cult of the lamb") || lower.Contains("hotline miami") || lower.Contains("enter the gungeon") || lower.Contains("katana zero") || lower.Contains("gris") || lower.Contains("inscryption") || lower.Contains("the messenger") || lower.Contains("loop hero") || lower.Contains("talos principle") || lower.Contains("shadow warrior") || lower.Contains("serious sam") || lower.Contains("broforce") || lower.Contains("carrion") || lower.Contains("death's door") || lower.Contains("ape out") || lower.Contains("my friend pedro") || lower.Contains("return to monkey island"))
                return "Devolver Digital";

            if (lower.Contains("stray") || lower.Contains("outer wilds") || lower.Contains("neon white") || lower.Contains("what remains of edith finch") || lower.Contains("cocoon") || lower.Contains("journey") || lower.Contains("solar ash") || lower.Contains("sayonara wild hearts") || lower.Contains("kentucky route zero") || lower.Contains("twelve minutes") || lower.Contains("as dusk falls") || lower.Contains("lorelei and the laser eyes") || lower.Contains("donut county") || lower.Contains("gorogoa"))
                return "Annapurna Interactive";

            if (lower.Contains("frostpunk") || lower.Contains("this war of mine") || lower.Contains("the alters") || lower.Contains("the invincible") || lower.Contains("indika") || lower.Contains("moonlighter") || lower.Contains("children of morta"))
                return "11 bit studios";

            if (lower.Contains("worms") || lower.Contains("overcooked") || lower.Contains("the escapists") || lower.Contains("hell let loose") || lower.Contains("blasphemous") || lower.Contains("dredge") || lower.Contains("moving out") || lower.Contains("trepang2") || lower.Contains("yooka-laylee"))
                return "Team17";

            if (lower.Contains("grid") || lower.Contains("dirt") || lower.Contains("f1 20") || lower.Contains("f1 22") || lower.Contains("f1 23") || lower.Contains("f1 24") || lower.Contains("operation flashpoint: dragon"))
                return "Codemasters";

            if (lower.Contains("firewatch"))
                return "Campo Santo";

            if (lower.Contains("war thunder") || lower.Contains("crossout") || lower.Contains("enlisted"))
                return "Gaijin Entertainment";

            if (lower.Contains("darkest dungeon"))
                return "Red Hook Studios";

            if (lower.Contains("syberia") || lower.Contains("garfield kart") || lower.Contains("tintin"))
                return "Microids";

            if (lower.Contains("torchlight") || lower.Contains("hob"))
                return "Runic Games";

            if (lower.Contains("euro truck simulator") || lower.Contains("american truck simulator"))
                return "SCS Software";

            if (lower.Contains("ftl: faster than light") || lower.Contains("faster than light") || lower.Contains("into the breach"))
                return "Subset Games";

            if (lower.Contains("postal"))
                return "Running With Scissors";

            if (lower.Contains("kingdom: classic") || lower.Contains("kingdom two crowns") || lower.Contains("kingdom eight") || lower.Contains("call of the sea") || lower.Contains("sable") || lower.Contains("norco") || lower.Contains("cassette beasts"))
                return "Raw Fury";

            if (lower.Contains("surviving mars") || lower.Contains("tropico") || lower.Contains("victor vran") || lower.Contains("jagged alliance 3"))
                return "Haemimont Games";

            if (lower.Contains("to the moon") || lower.Contains("finding paradise") || lower.Contains("impostor factory"))
                return "Freebird Games";

            if (lower.Contains("black mesa"))
                return "Crowbar Collective";

            if (lower.Contains("human: fall flat") || lower.Contains("the ascent") || lower.Contains("for the king") || lower.Contains("autonauts"))
                return "Curve Games";

            if (lower.Contains("doki doki literature club"))
                return "Team Salvato";

            if (lower.Contains("the long dark"))
                return "Hinterland Studio";

            if (lower.Contains("shadowrun") || lower.Contains("battletech") || lower.Contains("the lamplighters league"))
                return "Harebrained Schemes";

            if (lower.Contains("sniper ghost warrior") || lower.Contains("lords of the fallen"))
                return "CI Games";

            if (lower.Contains("vermintide") || lower.Contains("darktide") || lower.Contains("lead and gold"))
                return "Fatshark";

            if (lower.Contains("squad") || lower.Contains("starship troopers: extermination"))
                return "Offworld Industries";

            if (lower.Contains("chivalry"))
                return "Torn Banner Studios";

            if (lower.Contains("detroit: become human") || lower.Contains("heavy rain") || lower.Contains("beyond: two souls") || lower.Contains("fahrenheit"))
                return "Quantic Dream";

            if (lower.Contains("the walking dead") || lower.Contains("the wolf among us") || lower.Contains("batman: the telltale") || lower.Contains("tales from the borderlands"))
                return "Telltale Games";

            if (lower.Contains("outlast") || lower.Contains("the outlast trials"))
                return "Red Barrels";

            if (lower.Contains("warframe"))
                return "Digital Extremes";

            if (lower.Contains("super meat boy") || lower.Contains("the binding of isaac"))
                return "Team Meat";

            if (lower.Contains("fall guys"))
                return "Mediatonic";

            if (lower.Contains("rocket league"))
                return "Psyonix";

            if (lower.Contains("among us"))
                return "Innersloth";

            if (lower.Contains("paladins") || lower.Contains("smite") || lower.Contains("realm royale"))
                return "Hi-Rez Studios";

            if (lower.Contains("unturned"))
                return "Smartly Dressed Games";

            if (lower.Contains("brutal legend") || lower.Contains("psychonauts") || lower.Contains("broken age") || lower.Contains("grim fandango") || lower.Contains("full throttle") || lower.Contains("day of the tentacle"))
                return "Double Fine Productions";

            if (lower.Contains("black desert") || lower.Contains("crimson desert"))
                return "Pearl Abyss";

            if (lower.Contains("layers of fear") || lower.Contains("observer") || lower.Contains("the medium") || lower.Contains("blair witch"))
                return "Bloober Team";

            if (lower.Contains("ys") || lower.Contains("trails in the sky") || lower.Contains("trails of cold steel") || lower.Contains("legend of heroes"))
                return "Nihon Falcom";

            if (lower.Contains("king of fighters") || lower.Contains("metal slug") || lower.Contains("samurai shodown") || lower.Contains("fatal fury"))
                return "SNK";

            if (lower.Contains("guilty gear") || lower.Contains("blazblue") || lower.Contains("dragon ball fighterz") || lower.Contains("granblue fantasy versus"))
                return "Arc System Works";

            if (lower.Contains("disco elysium"))
                return "ZA/UM";

            if (lower.Contains("slay the spire"))
                return "Mega Crit";

            if (lower.Contains("vampire survivors"))
                return "poncle";

            if (lower.Contains("balatro") || lower.Contains("mortal shell") || lower.Contains("the last faith"))
                return "Playstack";

            if (lower.Contains("hollow knight") || lower.Contains("silksong"))
                return "Team Cherry";

            if (lower.Contains("hades") || lower.Contains("bastion") || lower.Contains("transistor") || lower.Contains("pyre"))
                return "Supergiant Games";

            if (lower.Contains("stardew valley"))
                return "ConcernedApe";

            if (lower.Contains("terraria"))
                return "Re-Logic";

            if (lower.Contains("celeste") || lower.Contains("towerfall"))
                return "Extremely OK Games";

            if (lower.Contains("dead cells") || lower.Contains("windblown"))
                return "Motion Twin";

            if (lower.Contains("undertale") || lower.Contains("deltarune"))
                return "Toby Fox";

            if (lower.Contains("cuphead"))
                return "Studio MDHR";

            if (lower.Contains("subnautica"))
                return "Unknown Worlds Entertainment";

            if (lower.Contains("rust") || lower.Contains("garry's mod"))
                return "Facepunch Studios";

            if (lower.Contains("valheim"))
                return "Iron Gate Studio";

            if (lower.Contains("deep rock galactic"))
                return "Ghost Ship Games";

            if ((lower.Contains("the forest") || lower.Contains("sons of the forest")) && !lower.Contains("blind forest"))
                return "Endnight Games";

            if (lower.Contains("limbo") || lower.Contains("inside"))
                return "Playdead";

            if (lower.Contains("dont starve") || lower.Contains("don't starve") || lower.Contains("oxygen not included") || lower.Contains("mark of the ninja") || lower.Contains("invisible inc"))
                return "Klei Entertainment";

            if (lower.Contains("no man's sky") || lower.Contains("light no fire") || lower.Contains("last campfire"))
                return "Hello Games";

            if (lower.Contains("satisfactory") || lower.Contains("goat simulator"))
                return "Coffee Stain Studios";

            if (lower.Contains("s.t.a.l.k.e.r.") || lower.Contains("stalker") || lower.Contains("cossacks"))
                return "GSC Game World";

            if (lower.Contains("hitman") || lower.Contains("kane & lynch") || lower.Contains("freedom fighters"))
                return "IO Interactive";

            if (lower.Contains("dying light") || lower.Contains("call of juarez"))
                return "Techland";

            if (lower.Contains("death stranding") || lower.Contains("p.t."))
                return "Kojima Productions";

            if (lower.Contains("destiny") || lower.Contains("marathon"))
                return "Bungie";

            if (lower.Contains("ori and the") || lower.Contains("no rest for the wicked"))
                return "Moon Studios";

            if (lower.Contains("pubg") || lower.Contains("the callisto protocol") || lower.Contains("inzoi"))
                return "KRAFTON";

            if (lower.Contains("the finals") || lower.Contains("arc raiders"))
                return "Embark Studios";

            if (lower.Contains("dead by daylight"))
                return "Behaviour Interactive";

            if (lower.Contains("crysis") || lower.Contains("hunt: showdown") || lower.Contains("ryse: son of rome"))
                return "Crytek";

            if (lower.Contains("black myth: wukong") || lower.Contains("black myth"))
                return "Game Science";

            if (lower.Contains("helldivers") || lower.Contains("magicka"))
                return "Arrowhead Game Studios";

            if (lower.Contains("palworld") || lower.Contains("craftopia"))
                return "Pocketpair";

            if (lower.Contains("space marine") || lower.Contains("snowrunner") || lower.Contains("mudrunner") || lower.Contains("world war z") || lower.Contains("evil dead"))
                return "Saber Interactive";

            if (lower.Contains("manor lords"))
                return "Slavic Magic";

            if (lower.Contains("ark: survival"))
                return "Studio Wildcard";

            if (lower.Contains("genshin impact") || lower.Contains("honkai") || lower.Contains("zenless zone zero"))
                return "HoYoverse";

            if (lower.Contains("league of legends") || lower.Contains("valorant") || lower.Contains("teamfight tactics") || lower.Contains("legends of runeterra"))
                return "Riot Games";

            if (lower.Contains("fortnite") || lower.Contains("unreal tournament") || lower.Contains("unreal"))
                return "Epic Games";

            if (lower.Contains("tunic") || lower.Contains("night in the woods") || lower.Contains("chicory"))
                return "Finji";

            if (lower.Contains("lies of p") || lower.Contains("skul: the hero slayer") || lower.Contains("sanabi"))
                return "Neowiz";

            if (lower.Contains("v rising") || lower.Contains("battlerite"))
                return "Stunlock Studios";

            if (lower.Contains("payday") || lower.Contains("enclave") || lower.Contains("syndicate"))
                return "Starbreeze Studios";

            if (lower.Contains("kingdom come"))
                return "Warhorse Studios";

            if (lower.Contains("killing floor") || lower.Contains("maneater") || lower.Contains("chivalry"))
                return "Tripwire Interactive";

            if (lower.Contains("mount & blade") || lower.Contains("bannerlord") || lower.Contains("warband"))
                return "TaleWorlds Entertainment";

            if (lower.Contains("sniper elite") || lower.Contains("zombie army") || lower.Contains("evil genius"))
                return "Rebellion Developments";

            if (lower.Contains("enshrouded") || lower.Contains("portal knights"))
                return "Keen Games";

            if (lower.Contains("astroneer"))
                return "System Era Softworks";

            if (lower.Contains("raft") || lower.Contains("scrap mechanic"))
                return "Axolot Games";

            if (lower.Contains("lethal company"))
                return "Zeekerss";

            if (lower.Contains("phasmophobia"))
                return "Kinetic Games";

            if (lower.Contains("content warning") || lower.Contains("totally accurate") || lower.Contains("tabs") || lower.Contains("clustertruck"))
                return "Landfall";

            if (lower.Contains("slime rancher"))
                return "Monomi Park";

            if (lower.Contains("outer worlds") || lower.Contains("kerbal space program") || lower.Contains("rollerdrome") || lower.Contains("olliolli") || lower.Contains("ancestors: the humankind") || lower.Contains("penny's big breakaway"))
                return "Private Division";

            if (lower.Contains("sifu") || lower.Contains("pacific drive") || lower.Contains("scorn") || lower.Contains("flintlock") || lower.Contains("cat quest") || lower.Contains("tchia"))
                return "Kepler Interactive";

            if (lower.Contains("ultrakill") || lower.Contains("dusk") || lower.Contains("amid evil") || lower.Contains("gloomwood") || lower.Contains("faith: the unholy trinity"))
                return "New Blood Interactive";

            if (lower.Contains("arma") || lower.Contains("dayz") || lower.Contains("operation flashpoint"))
                return "Bohemia Interactive";

            if (lower.Contains("northgard") || lower.Contains("wartales") || lower.Contains("dune: spice wars"))
                return "Shiro Games";

            if (lower.Contains("factorio"))
                return "Wube Software";

            if (lower.Contains("rimworld"))
                return "Ludeon Studios";

            if (lower.Contains("papers, please") || lower.Contains("return of the obra dinn"))
                return "3909 LLC";

            if (lower.Contains("the stanley parable") || lower.Contains("the beginner's guide"))
                return "Galactic Cafe";

            if (lower.Contains("superhot"))
                return "SUPERHOT Team";

            if (lower.Contains("spelunky") || lower.Contains("ufo 50"))
                return "Mossmouth";

            if (lower.Contains("braid") || lower.Contains("the witness"))
                return "Thekla";

            if (lower.Contains("shovel knight") || lower.Contains("mina the hollower"))
                return "Yacht Club Games";

            if (lower.Contains("guacamelee") || lower.Contains("nobody saves the world"))
                return "DrinkBox Studios";

            if (lower.Contains("castle crashers") || lower.Contains("battleblock theater") || lower.Contains("pit people"))
                return "The Behemoth";

            if (lower.Contains("sea of stars"))
                return "Sabotage Studio";

            if (lower.Contains("dave the diver"))
                return "MINTROCKET";

            if (lower.Contains("path of exile"))
                return "Grinding Gear Games";

            if (lower.Contains("amnesia") || lower.Contains("soma") || lower.Contains("penumbra"))
                return "Frictional Games";

            if (lower.Contains("frogwares") || lower.Contains("sherlock holmes") || lower.Contains("the sinking city"))
                return "Frogwares";

            if (lower.Contains("nacon") || lower.Contains("bigben") || lower.Contains("robocop: rogue city") || lower.Contains("wrc") || lower.Contains("styx") || lower.Contains("werewolf: the apocalypse"))
                return "Nacon";

            if (lower.Contains("tinybuild") || lower.Contains("hello neighbor") || lower.Contains("graveyard keeper") || lower.Contains("speedrunners") || lower.Contains("streets of rogue") || lower.Contains("potion craft") || lower.Contains("party hard"))
                return "tinyBuild";

            if (lower.Contains("chucklefish") || lower.Contains("starbound") || lower.Contains("wargroove") || lower.Contains("eastward") || lower.Contains("inmost"))
                return "Chucklefish";

            if (lower.Contains("daedalic") || lower.Contains("deponia") || lower.Contains("shadow tactics") || lower.Contains("barotrauma"))
                return "Daedalic Entertainment";

            if (lower.Contains("frontier developments") || lower.Contains("elite dangerous") || lower.Contains("planet coaster") || lower.Contains("planet zoo") || lower.Contains("jurassic world evolution") || lower.Contains("f1 manager"))
                return "Frontier Developments";

            if (lower.Contains("milestone") || lower.Contains("motogp") || lower.Contains("ride ") || lower.Contains("hot wheels unleashed") || lower.Contains("supercross"))
                return "Milestone";

            if (lower.Contains("dontnod") || lower.Contains("jusant") || lower.Contains("tell me why") || lower.Contains("remember me") || lower.Contains("harmony: the fall"))
                return "DON'T NOD";

            if (lower.Contains("supermassive") || lower.Contains("the dark pictures") || lower.Contains("the quarry") || lower.Contains("the casting of frank stone"))
                return "Supermassive Games";

            if (lower.Contains("shift up") || lower.Contains("stellar blade") || lower.Contains("goddess of victory"))
                return "Shift Up";

            if (lower.Contains("level-5") || lower.Contains("professor layton") || lower.Contains("inazuma eleven") || lower.Contains("yo-kai watch"))
                return "Level-5";

            if (lower.Contains("marvelous") || lower.Contains("rune factory") || lower.Contains("story of seasons") || lower.Contains("daemon x machina"))
                return "Marvelous";

            if (lower.Contains("nis america") || lower.Contains("disgaea"))
                return "NIS America";

            if (lower.Contains("atari") || lower.Contains("rollercoaster tycoon") || lower.Contains("alone in the dark"))
                return "Atari";

            if (lower.Contains("lucasarts") || lower.Contains("monkey island") || lower.Contains("star wars: force unleashed") || lower.Contains("star wars: republic commando") || lower.Contains("star wars: empire at war") || lower.Contains("star wars jedi knight"))
                return "LucasArts";

            if (lower.Contains("sierra") || lower.Contains("king's quest") || lower.Contains("space quest") || lower.Contains("caesar") || lower.Contains("pharaoh") || lower.Contains("swat 4") || lower.Contains("empire earth") || lower.Contains("homeworld"))
                return "Sierra Entertainment";

            if (lower.Contains("obsidian") || lower.Contains("avowed") || lower.Contains("pillars of eternity") || lower.Contains("tyranny") || lower.Contains("pentiment"))
                return "Obsidian Entertainment";

            if (lower.Contains("bioware") || lower.Contains("neverwinter nights") || lower.Contains("jade empire"))
                return "BioWare";

            if (lower.Contains("id software") || lower.Contains("rage 2"))
                return "id Software";

            if (lower.Contains("arkane") || lower.Contains("deathloop") || lower.Contains("redfall") || lower.Contains("arx fatalis"))
                return "Arkane Studios";

            // Se o jogo não pertencer a nenhum estúdio verificado/oficial, rejeitar como shovelware/lixo
            return null;
        }

        private async Task<string?> ResolverEstudioCompletoAsync(RawgGameItemDTO rawg, SemaphoreSlim detailSemaphore)
        {
            var studio = ResolverEstudio(rawg);
            if (!string.IsNullOrWhiteSpace(studio))
            {
                return studio;
            }

            await detailSemaphore.WaitAsync();
            try
            {
                var detail = await _rawgService.ObterDetalhesJogoExterno(rawg.Id);
                if (detail != null)
                {
                    var pub = detail.Publishers?.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Name))?.Name?.Trim();
                    if (!string.IsNullOrWhiteSpace(pub) && pub.Length > 1 && !pub.Contains("Independente", StringComparison.OrdinalIgnoreCase))
                    {
                        return pub.Length > 150 ? pub.Substring(0, 150).Trim() : pub;
                    }

                    var dev = detail.Developers?.FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.Name))?.Name?.Trim();
                    if (!string.IsNullOrWhiteSpace(dev) && dev.Length > 1 && !dev.Contains("Independente", StringComparison.OrdinalIgnoreCase))
                    {
                        return dev.Length > 150 ? dev.Substring(0, 150).Trim() : dev;
                    }
                }
            }
            catch
            {
                // Ignorar falha assíncrona
            }
            finally
            {
                detailSemaphore.Release();
            }

            return null;
        }

        private static string MapearGeneroInglesParaPortugues(string generoIngles)
        {
            return generoIngles?.Trim().ToLowerInvariant() switch
            {
                "action" => "Ação",
                "role-playing-games-rpg" or "rpg" => "RPG",
                "adventure" => "Aventura",
                "shooter" => "Tiro",
                "strategy" => "Estratégia",
                "puzzle" => "Quebra-Cabeça",
                "racing" => "Corrida",
                "fighting" => "Luta",
                "sports" => "Esportes",
                "indie" => "Indie",
                "simulation" => "Simulação",
                "massively-multiplayer" or "mmo" => "MMORPG",
                "platformer" => "Plataforma",
                "casual" => "Casual",
                "arcade" => "Ação",
                "family" => "Casual",
                "card" or "board-games" => "Estratégia",
                "educational" => "Casual",
                _ => "Ação"
            };
        }

        private static List<string> ObterListaEstudiosFundamentais()
        {
            return new List<string>
            {
                "Rockstar Games",
                "FromSoftware",
                "Nintendo",
                "CD Projekt Red",
                "PlayStation Studios",
                "Xbox Game Studios",
                "Capcom",
                "Square Enix",
                "Bethesda Softworks",
                "Larian Studios",
                "Remedy Entertainment",
                "Ubisoft",
                "Electronic Arts",
                "Activision Blizzard",
                "Sega",
                "Atlus",
                "Bandai Namco",
                "Valve Corporation",
                "Warner Bros. Games",
                "2K Games",
                "Konami",
                "Deep Silver / THQ Nordic",
                "Focus Entertainment",
                "505 Games",
                "Paradox Interactive",
                "Koei Tecmo",
                "Spike Chunsoft",
                "Devolver Digital",
                "Annapurna Interactive",
                "11 bit studios",
                "Team17",
                "Codemasters",
                "Campo Santo",
                "Gaijin Entertainment",
                "Red Hook Studios",
                "Microids",
                "Runic Games",
                "SCS Software",
                "Subset Games",
                "Running With Scissors",
                "Raw Fury",
                "Haemimont Games",
                "Freebird Games",
                "Crowbar Collective",
                "Curve Games",
                "Team Salvato",
                "Hinterland Studio",
                "Harebrained Schemes",
                "CI Games",
                "Fatshark",
                "Offworld Industries",
                "Torn Banner Studios",
                "Quantic Dream",
                "Telltale Games",
                "Red Barrels",
                "Digital Extremes",
                "Team Meat",
                "Mediatonic",
                "Psyonix",
                "Innersloth",
                "Hi-Rez Studios",
                "Smartly Dressed Games",
                "Double Fine Productions",
                "Pearl Abyss",
                "Bloober Team",
                "Nihon Falcom",
                "SNK",
                "Arc System Works",
                "ZA/UM",
                "Mega Crit",
                "poncle",
                "Playstack",
                "Team Cherry",
                "Supergiant Games",
                "ConcernedApe",
                "Re-Logic",
                "Extremely OK Games",
                "Motion Twin",
                "Toby Fox",
                "Studio MDHR",
                "Unknown Worlds Entertainment",
                "Facepunch Studios",
                "Iron Gate Studio",
                "Ghost Ship Games",
                "Endnight Games",
                "Playdead",
                "Klei Entertainment",
                "Hello Games",
                "Coffee Stain Studios",
                "GSC Game World",
                "IO Interactive",
                "Techland",
                "Kojima Productions",
                "Bungie",
                "Moon Studios",
                "KRAFTON",
                "Embark Studios",
                "Behaviour Interactive",
                "Crytek",
                "Game Science",
                "Arrowhead Game Studios",
                "Pocketpair",
                "Saber Interactive",
                "Slavic Magic",
                "Studio Wildcard",
                "HoYoverse",
                "Riot Games",
                "Epic Games",
                "Finji",
                "Neowiz",
                "Stunlock Studios",
                "Starbreeze Studios",
                "Warhorse Studios",
                "Tripwire Interactive",
                "TaleWorlds Entertainment",
                "Rebellion Developments",
                "Keen Games",
                "System Era Softworks",
                "Axolot Games",
                "Zeekerss",
                "Kinetic Games",
                "Landfall",
                "Monomi Park",
                "Private Division",
                "Kepler Interactive",
                "New Blood Interactive",
                "Bohemia Interactive",
                "Shiro Games",
                "Wube Software",
                "Ludeon Studios",
                "3909 LLC",
                "Galactic Cafe",
                "SUPERHOT Team",
                "Mossmouth",
                "Thekla",
                "Yacht Club Games",
                "DrinkBox Studios",
                "The Behemoth",
                "Sabotage Studio",
                "MINTROCKET",
                "Grinding Gear Games",
                "Frictional Games",
                "Nightdive Studios",
                "Sukeban Games / Ysbryd Games",
                "Frogwares",
                "Nacon",
                "tinyBuild",
                "Chucklefish",
                "Daedalic Entertainment",
                "Frontier Developments",
                "Milestone",
                "DON'T NOD",
                "Supermassive Games",
                "Shift Up",
                "Level-5",
                "Marvelous",
                "NIS America",
                "Atari",
                "LucasArts",
                "Sierra Entertainment",
                "Obsidian Entertainment",
                "BioWare",
                "id Software",
                "Arkane Studios"
            };
        }
    }
}
