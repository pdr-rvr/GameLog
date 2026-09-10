using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GameLog_Backend.Helpers
{
    public static class RelevanciaBuscaHelper
    {
        private static readonly HashSet<string> StopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "o", "as", "os", "um", "uma", "uns", "umas",
            "de", "do", "da", "dos", "das", "em", "no", "na", "nos", "nas",
            "por", "para", "com", "e", "ou",
            "the", "of", "and", "or", "in", "on", "at", "to", "for", "with", "by", "from", "an", "is"
        };

        public static string RemoverAcentos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            var normalizedString = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public static List<string> ObterTermosSignificativos(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<string>();

            var limpo = RemoverAcentos(query.Trim().ToLowerInvariant());
            var tokens = Regex.Split(limpo, @"[^\p{L}\p{N}]+")
                              .Where(t => !string.IsNullOrWhiteSpace(t))
                              .ToList();

            var significativos = tokens.Where(t => !StopWords.Contains(t)).ToList();
            return significativos.Any() ? significativos : tokens;
        }

        public static bool CorrespondeBusca(string? titulo, string? nomeEmpresa, string query)
            => CorrespondeBusca(titulo, nomeEmpresa, null, query);

        public static bool CorrespondeBusca(string? titulo, string? nomeDesenvolvedora, string? nomePublicadora, string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;

            var qNorm = RemoverAcentos(query.Trim().ToLowerInvariant());
            if (string.IsNullOrWhiteSpace(qNorm)) return true;

            // 1. Se a desenvolvedora corresponder à busca
            if (!string.IsNullOrWhiteSpace(nomeDesenvolvedora))
            {
                var devNorm = RemoverAcentos(nomeDesenvolvedora.Trim().ToLowerInvariant());
                if (devNorm.Contains(qNorm)) return true;
            }

            // 2. Se a publicadora corresponder à busca
            if (!string.IsNullOrWhiteSpace(nomePublicadora))
            {
                var pubNorm = RemoverAcentos(nomePublicadora.Trim().ToLowerInvariant());
                if (pubNorm.Contains(qNorm)) return true;
            }

            if (string.IsNullOrWhiteSpace(titulo)) return false;

            var tNorm = RemoverAcentos(titulo.Trim().ToLowerInvariant());

            // 2. Se o título contiver a frase exata
            if (tNorm.Contains(qNorm)) return true;

            // 3. Checagem de termos significativos
            var termosQuery = ObterTermosSignificativos(query);
            if (!termosQuery.Any()) return true;

            var tokensTitulo = new HashSet<string>(
                Regex.Split(tNorm, @"[^\p{L}\p{N}]+").Where(t => !string.IsNullOrWhiteSpace(t)),
                StringComparer.OrdinalIgnoreCase
            );

            // Para que o título seja considerado correspondente, TODOS os termos significativos da busca
            // devem estar presentes no título (como palavra completa ou prefixo de palavra).
            return termosQuery.All(termo => 
                tokensTitulo.Any(tok => tok.StartsWith(termo, StringComparison.OrdinalIgnoreCase) || tok.Equals(termo, StringComparison.OrdinalIgnoreCase)) 
                || tNorm.Contains(termo)
            );
        }

        public static int CalcularScoreRelevancia(string? titulo, string query)
        {
            if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(query)) return 0;

            var tNorm = RemoverAcentos(titulo.Trim().ToLowerInvariant());
            var qNorm = RemoverAcentos(query.Trim().ToLowerInvariant());

            // Match exato
            if (tNorm == qNorm) return 10000;

            // Começa exatamente com a consulta (ex: "God of War: Ragnarök" para "God of War")
            if (tNorm.StartsWith(qNorm)) return 8000;

            // Contém a frase exata da consulta (ex: "The Legend of Zelda" para "Zelda")
            if (tNorm.Contains(qNorm)) return 6000;

            var termosQuery = ObterTermosSignificativos(query);
            if (termosQuery.Count > 1 && termosQuery.All(termo => tNorm.Contains(termo)))
            {
                return 4000;
            }

            if (termosQuery.Any(termo => tNorm.Contains(termo)))
            {
                return 1000;
            }

            return 0;
        }
    }
}
