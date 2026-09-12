using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLog_Backend.Services.Catalog
{
    public static class GenreTaxonomyService
    {
        // Lista consolidada e expandida de Categorias Canônicas (Gêneros e Subgêneros unificados)
        public static readonly string[] CategoriasCanonicas = new[]
        {
            "Ação",
            "Aventura",
            "RPG",
            "RPG de Ação",
            "JRPG",
            "Soulslike",
            "Metroidvania",
            "Roguelike / Roguelite",
            "Terror & Sobrevivência",
            "Survival Horror",
            "Mundo Aberto",
            "Hack and Slash",
            "Plataforma",
            "Tiro (FPS / TPS)",
            "Estratégia",
            "Estratégia em Turnos",
            "RTS (Tempo Real)",
            "Ficção Científica & Cyberpunk",
            "Fantasia Medieval",
            "Stealth",
            "Corrida",
            "Simulação",
            "Luta",
            "Quebra-Cabeça",
            "Esportes",
            "Point & Click",
            "Visual Novel",
            "Battle Royale",
            "MMORPG",
            "Ritmo / Musical",
            "Indie"
        };

        // Mapeamento direto de Gêneros da RAWG para Categorias Canônicas
        private static readonly Dictionary<string, string> RawgGenreMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "action", "Ação" },
            { "adventure", "Aventura" },
            { "role-playing-games-rpg", "RPG" },
            { "strategy", "Estratégia" },
            { "shooter", "Tiro (FPS / TPS)" },
            { "racing", "Corrida" },
            { "sports", "Esportes" },
            { "fighting", "Luta" },
            { "puzzle", "Quebra-Cabeça" },
            { "simulation", "Simulação" },
            { "platformer", "Plataforma" },
            { "arcade", "Ação" },
            { "massively-multiplayer", "MMORPG" },
            { "indie", "Indie" },
            { "casual", "Aventura" }
        };

        // Mapeamento inteligente de Tags / Termos da RAWG para Subgêneros e Categorias Especializadas
        private static readonly Dictionary<string, string> TagToCategoryMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "souls-like", "Soulslike" },
            { "soulslike", "Soulslike" },
            { "dark-souls", "Soulslike" },
            { "metroidvania", "Metroidvania" },
            { "roguelike", "Roguelike / Roguelite" },
            { "rogue-like", "Roguelike / Roguelite" },
            { "roguelite", "Roguelike / Roguelite" },
            { "rogue-lite", "Roguelike / Roguelite" },
            { "action-rpg", "RPG de Ação" },
            { "arpg", "RPG de Ação" },
            { "jrpg", "JRPG" },
            { "japanese-rpg", "JRPG" },
            { "open-world", "Mundo Aberto" },
            { "open-world-2", "Mundo Aberto" },
            { "hack-and-slash", "Hack and Slash" },
            { "hack-slash", "Hack and Slash" },
            { "spectacle-fighter", "Hack and Slash" },
            { "survival-horror", "Survival Horror" },
            { "horror", "Terror & Sobrevivência" },
            { "psychological-horror", "Terror & Sobrevivência" },
            { "fps", "Tiro (FPS / TPS)" },
            { "first-person-shooter", "Tiro (FPS / TPS)" },
            { "third-person-shooter", "Tiro (FPS / TPS)" },
            { "tps", "Tiro (FPS / TPS)" },
            { "cyberpunk", "Ficção Científica & Cyberpunk" },
            { "sci-fi", "Ficção Científica & Cyberpunk" },
            { "science-fiction", "Ficção Científica & Cyberpunk" },
            { "space", "Ficção Científica & Cyberpunk" },
            { "medieval", "Fantasia Medieval" },
            { "dark-fantasy", "Fantasia Medieval" },
            { "high-fantasy", "Fantasia Medieval" },
            { "stealth", "Stealth" },
            { "turn-based", "Estratégia em Turnos" },
            { "turn-based-strategy", "Estratégia em Turnos" },
            { "tactical-rpg", "Estratégia em Turnos" },
            { "tactical", "Estratégia em Turnos" },
            { "rts", "RTS (Tempo Real)" },
            { "real-time-strategy", "RTS (Tempo Real)" },
            { "point-click", "Point & Click" },
            { "point-and-click", "Point & Click" },
            { "visual-novel", "Visual Novel" },
            { "battle-royale", "Battle Royale" },
            { "mmo", "MMORPG" },
            { "mmorpg", "MMORPG" },
            { "rhythm", "Ritmo / Musical" },
            { "music", "Ritmo / Musical" }
        };

        // Tags genéricas / lixo a serem ignoradas
        private static readonly HashSet<string> IgnoredTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "singleplayer", "multiplayer", "co-op", "full-controller-support",
            "steam-achievements", "steam-trading-cards", "steam-cloud", "great-soundtrack",
            "atmospheric", "story-rich", "difficult", "3d", "2d", "exploration",
            "short", "first-person", "third-person", "casual", "funny", "female-protagonist"
        };

        /// <summary>
        /// Mapeia e normaliza uma lista de gêneros e tags brutas da RAWG para a lista canônica do GameLog.
        /// </summary>
        public static List<string> MapearGenerosETags(IEnumerable<string>? rawgGenres, IEnumerable<string>? rawgTags, string? tituloJogo = null)
        {
            var resultado = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Processar gêneros base
            if (rawgGenres != null)
            {
                foreach (var g in rawgGenres)
                {
                    if (string.IsNullOrWhiteSpace(g)) continue;
                    var slug = g.Trim().ToLowerInvariant().Replace(" ", "-");
                    
                    if (RawgGenreMap.TryGetValue(slug, out var catCanonica))
                    {
                        resultado.Add(catCanonica);
                    }
                    else if (RawgGenreMap.TryGetValue(g.Trim(), out var catCanonicaNome))
                    {
                        resultado.Add(catCanonicaNome);
                    }
                    else
                    {
                        var norm = NormalizarTextoSimples(g);
                        if (!string.IsNullOrWhiteSpace(norm))
                        {
                            resultado.Add(norm);
                        }
                    }
                }
            }

            // 2. Processar tags para inferir subgêneros e categorias especializadas
            if (rawgTags != null)
            {
                foreach (var tag in rawgTags)
                {
                    if (string.IsNullOrWhiteSpace(tag)) continue;
                    var tagSlug = tag.Trim().ToLowerInvariant().Replace(" ", "-");
                    
                    if (IgnoredTags.Contains(tagSlug)) continue;

                    if (TagToCategoryMap.TryGetValue(tagSlug, out var subCanonica))
                    {
                        resultado.Add(subCanonica);
                    }
                }
            }

            // 3. Inferência contextual por Franquia / Título (para títulos famosos garantirem subgêneros canônicos)
            if (!string.IsNullOrWhiteSpace(tituloJogo))
            {
                var t = tituloJogo.ToLowerInvariant();
                if (t.Contains("dark souls") || t.Contains("bloodborne") || t.Contains("elden ring") || t.Contains("sekiro") || t.Contains("lies of p"))
                {
                    resultado.Add("Soulslike");
                    resultado.Add("RPG de Ação");
                }
                else if (t.Contains("hollow knight") || t.Contains("castlevania") || t.Contains("metroid") || t.Contains("ori and"))
                {
                    resultado.Add("Metroidvania");
                }
                else if (t.Contains("cyberpunk 2077") || t.Contains("deus ex"))
                {
                    resultado.Add("Ficção Científica & Cyberpunk");
                }
                else if (t.Contains("resident evil") || t.Contains("silent hill") || t.Contains("dead space"))
                {
                    resultado.Add("Survival Horror");
                    resultado.Add("Terror & Sobrevivência");
                }
                else if (t.Contains("devil may cry") || t.Contains("bayonetta") || t.Contains("god of war"))
                {
                    resultado.Add("Hack and Slash");
                }
                else if (t.Contains("persona") || t.Contains("final fantasy") || t.Contains("dragon quest") || t.Contains("tales of"))
                {
                    resultado.Add("JRPG");
                    resultado.Add("RPG");
                }
            }

            // Garantir que nenhum jogo fique sem categoria
            if (!resultado.Any())
            {
                resultado.Add("Ação");
            }

            return resultado.OrderBy(c => c).ToList();
        }

        public static string NormalizarTextoSimples(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "Ação";
            var lower = texto.Trim().ToLowerInvariant();

            if (lower.Contains("action") || lower.Contains("acao") || lower.Contains("aao")) return "Ação";
            if (lower.Contains("adventure") || lower.Contains("aventura")) return "Aventura";
            if (lower.Contains("role-playing") || lower.Contains("rpg")) return "RPG";
            if (lower.Contains("shooter") || lower.Contains("tiro") || lower.Contains("fps")) return "Tiro (FPS / TPS)";
            if (lower.Contains("strategy") || lower.Contains("estrategia")) return "Estratégia";
            if (lower.Contains("racing") || lower.Contains("corrida")) return "Corrida";
            if (lower.Contains("sports") || lower.Contains("esportes")) return "Esportes";
            if (lower.Contains("fighting") || lower.Contains("luta")) return "Luta";
            if (lower.Contains("puzzle") || lower.Contains("quebra-cabeca")) return "Quebra-Cabeça";
            if (lower.Contains("simulation") || lower.Contains("simulacao")) return "Simulação";
            if (lower.Contains("horror") || lower.Contains("terror")) return "Terror & Sobrevivência";
            if (lower.Contains("platform") || lower.Contains("plataforma")) return "Plataforma";
            if (lower.Contains("indie")) return "Indie";

            return char.ToUpper(texto[0]) + texto.Substring(1);
        }

        public static string MapearGenero(string? rawgGenre)
        {
            if (string.IsNullOrWhiteSpace(rawgGenre)) return "Ação";
            var slug = rawgGenre.Trim().ToLowerInvariant().Replace(" ", "-");
            if (RawgGenreMap.TryGetValue(slug, out var cat)) return cat;
            if (TagToCategoryMap.TryGetValue(slug, out var tagCat)) return tagCat;
            return NormalizarTextoSimples(rawgGenre);
        }

        public static List<string> MapearGeneros(IEnumerable<string>? rawgGenres)
        {
            return MapearGenerosETags(rawgGenres, null);
        }
    }
}
