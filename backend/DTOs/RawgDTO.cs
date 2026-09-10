using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GameLog_Backend.DTOs
{
    public class RawgResponseDTO<T>
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }

        [JsonPropertyName("results")]
        public List<T> Results { get; set; } = new();
    }

    public class RawgNamedEntityDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string? Slug { get; set; }
    }

    public class RawgEsrbRatingDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("slug")]
        public string? Slug { get; set; }
    }

    public class RawgGameItemDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("released")]
        public string? Released { get; set; }

        [JsonPropertyName("background_image")]
        public string? BackgroundImage { get; set; }

        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        [JsonPropertyName("ratings_count")]
        public int? RatingsCount { get; set; }

        [JsonPropertyName("added")]
        public int? Added { get; set; }

        [JsonPropertyName("metacritic")]
        public int? Metacritic { get; set; }

        [JsonPropertyName("genres")]
        public List<RawgNamedEntityDTO> Genres { get; set; } = new();

        [JsonPropertyName("publishers")]
        public List<RawgNamedEntityDTO> Publishers { get; set; } = new();

        [JsonPropertyName("developers")]
        public List<RawgNamedEntityDTO> Developers { get; set; } = new();

        [JsonPropertyName("esrb_rating")]
        public RawgEsrbRatingDTO? EsrbRating { get; set; }

        [JsonPropertyName("parents_count")]
        public int ParentsCount { get; set; }

        [JsonPropertyName("additions_count")]
        public int AdditionsCount { get; set; }
    }

    public class RawgGameDetailDTO : RawgGameItemDTO
    {
        [JsonPropertyName("description_raw")]
        public string? DescriptionRaw { get; set; }

        [JsonPropertyName("description")]
        public string? DescriptionHtml { get; set; }

        [JsonPropertyName("website")]
        public string? Website { get; set; }
    }

    public class RawgSearchResultItemDTO
    {
        public int RawgId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Imagem { get; set; }
        public int? AnoLancamento { get; set; }
        public string? DataLancamento { get; set; }
        public string? NomeEmpresa { get; set; }
        public List<string> Generos { get; set; } = new();
        public double? NotaRawg { get; set; }
        public bool JaImportado { get; set; }
        public int? LocalJogoId { get; set; }
    }

    public class RawgSearchResultDTO
    {
        public int TotalResultados { get; set; }
        public int PaginaAtual { get; set; }
        public List<RawgSearchResultItemDTO> Jogos { get; set; } = new();
    }
}
