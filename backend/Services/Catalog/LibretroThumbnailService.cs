using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GameLog_Backend.Services.Catalog
{
    public static class LibretroThumbnailService
    {
        private const string LibretroBaseUrl = "https://thumbnails.libretro.com";

        private static readonly Dictionary<string, string> PlatformSystemMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // Nintendo
            { "nes", "Nintendo - Nintendo Entertainment System" },
            { "nintendo-entertainment-system", "Nintendo - Nintendo Entertainment System" },
            { "snes", "Nintendo - Super Nintendo Entertainment System" },
            { "super-nintendo", "Nintendo - Super Nintendo Entertainment System" },
            { "nintendo-64", "Nintendo - Nintendo 64" },
            { "n64", "Nintendo - Nintendo 64" },
            { "gamecube", "Nintendo - GameCube" },
            { "wii", "Nintendo - Wii" },
            { "wii-u", "Nintendo - Wii U" },
            { "game-boy", "Nintendo - Game Boy" },
            { "game-boy-color", "Nintendo - Game Boy Color" },
            { "game-boy-advance", "Nintendo - Game Boy Advance" },
            { "nintendo-ds", "Nintendo - Nintendo DS" },
            { "nintendo-3ds", "Nintendo - Nintendo 3DS" },

            // Sony PlayStation
            { "playstation", "Sony - PlayStation" },
            { "ps1", "Sony - PlayStation" },
            { "psx", "Sony - PlayStation" },
            { "playstation-2", "Sony - PlayStation 2" },
            { "ps2", "Sony - PlayStation 2" },
            { "playstation-3", "Sony - PlayStation 3" },
            { "ps3", "Sony - PlayStation 3" },
            { "playstation-portable", "Sony - PlayStation Portable" },
            { "psp", "Sony - PlayStation Portable" },
            { "playstation-vita", "Sony - PlayStation Vita" },
            { "ps-vita", "Sony - PlayStation Vita" },

            // Sega
            { "sega-master-system", "Sega - Master System - Mark III" },
            { "sega-genesis", "Sega - Mega Drive - Genesis" },
            { "genesis", "Sega - Mega Drive - Genesis" },
            { "mega-drive", "Sega - Mega Drive - Genesis" },
            { "sega-saturn", "Sega - Saturn" },
            { "dreamcast", "Sega - Dreamcast" },
            { "game-gear", "Sega - Game Gear" },

            // Microsoft & Atari
            { "xbox", "Microsoft - Xbox" },
            { "atari-2600", "Atari - 2600" },
            { "atari-7800", "Atari - 7800" },
            { "neo-geo", "SNK - Neo Geo" }
        };

        public static string? ObterUrlBoxart(string? tituloJogo, string? platformSlugOuNome, int? anoLancamento = null)
        {
            if (string.IsNullOrWhiteSpace(tituloJogo)) return null;

            string? sistema = null;
            if (!string.IsNullOrWhiteSpace(platformSlugOuNome))
            {
                var cleanPlat = platformSlugOuNome.Trim().ToLowerInvariant();
                PlatformSystemMap.TryGetValue(cleanPlat, out sistema);
            }

            // Se não informou plataforma mas o jogo é pré-2004, inferir sistema por títulos lendários
            if (string.IsNullOrWhiteSpace(sistema) && anoLancamento.HasValue && anoLancamento.Value < 2004)
            {
                sistema = InferirSistemaPorTituloEAno(tituloJogo, anoLancamento.Value);
            }

            if (string.IsNullOrWhiteSpace(sistema)) return null;

            var nomeFormatado = SanitizarNomeParaLibretro(tituloJogo);
            if (string.IsNullOrWhiteSpace(nomeFormatado)) return null;

            var sistemaEscapado = Uri.EscapeDataString(sistema);
            var nomeEscapado = Uri.EscapeDataString(nomeFormatado);

            return $"{LibretroBaseUrl}/{sistemaEscapado}/Named_Boxarts/{nomeEscapado}.png";
        }

        public static string SanitizarNomeParaLibretro(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo)) return string.Empty;

            var clean = titulo.Trim();
            // Caracteres especiais que o Libretro substitui por '_'
            clean = clean.Replace("&", "_")
                         .Replace("*", "_")
                         .Replace("/", "_")
                         .Replace(":", "_")
                         .Replace("`", "_")
                         .Replace("<", "_")
                         .Replace(">", "_")
                         .Replace("?", "_")
                         .Replace("\\", "_")
                         .Replace("|", "_")
                         .Replace("\"", "_");

            return clean;
        }

        private static string? InferirSistemaPorTituloEAno(string titulo, int ano)
        {
            var lower = titulo.ToLowerInvariant();

            if (lower.Contains("super mario 64") || lower.Contains("zelda: ocarina") || lower.Contains("zelda: majora") || lower.Contains("banjo") || lower.Contains("goldeneye"))
                return "Nintendo - Nintendo 64";

            if (lower.Contains("super mario world") || lower.Contains("chrono trigger") || lower.Contains("super metroid") || lower.Contains("donkey kong country"))
                return "Nintendo - Super Nintendo Entertainment System";

            if (lower.Contains("super mario bros") || lower.Contains("the legend of zelda") || lower.Contains("metroid (1986)"))
                return "Nintendo - Nintendo Entertainment System";

            if (lower.Contains("final fantasy vii") || lower.Contains("final fantasy viii") || lower.Contains("final fantasy ix") || lower.Contains("metal gear solid") || lower.Contains("crash bandicoot") || lower.Contains("silent hill (1999)") || lower.Contains("castlevania: symphony"))
                return "Sony - PlayStation";

            if (lower.Contains("shadow of the colossus") || lower.Contains("god of war (2005)") || lower.Contains("god of war ii") || lower.Contains("grand theft auto: san andreas") || lower.Contains("silent hill 2"))
                return "Sony - PlayStation 2";

            if (lower.Contains("sonic the hedgehog") || lower.Contains("streets of rage"))
                return "Sega - Mega Drive - Genesis";

            if (ano < 1990) return "Nintendo - Nintendo Entertainment System";
            if (ano <= 1995) return "Nintendo - Super Nintendo Entertainment System";
            if (ano <= 2000) return "Sony - PlayStation";
            if (ano <= 2005) return "Sony - PlayStation 2";

            return null;
        }
    }
}
