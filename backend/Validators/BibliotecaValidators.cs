using System.Linq;
using FluentValidation;
using GameLog_Backend.DTOs;
using GameLog_Backend.Entities;

namespace GameLog_Backend.Validators
{
    public class SalvarItemBibliotecaDTOValidator : AbstractValidator<SalvarItemBibliotecaDTO>
    {
        public SalvarItemBibliotecaDTOValidator()
        {
            RuleFor(x => x.JogoId)
                .NotEmpty().WithMessage("O identificador do jogo é obrigatório.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Status do jogo inválido.");
        }
    }

    public class ItemFavoritoPosicaoDTOValidator : AbstractValidator<ItemFavoritoPosicaoDTO>
    {
        public ItemFavoritoPosicaoDTOValidator()
        {
            RuleFor(x => x.Posicao)
                .InclusiveBetween(1, 5).WithMessage("A posição do favorito deve estar entre 1 e 5.");

            RuleFor(x => x.JogoId)
                .NotEmpty().WithMessage("Identificador de jogo inválido.");
        }
    }

    public class SalvarJogosFavoritosDTOValidator : AbstractValidator<SalvarJogosFavoritosDTO>
    {
        public SalvarJogosFavoritosDTOValidator()
        {
            RuleForEach(x => x.Favoritos)
                .SetValidator(new ItemFavoritoPosicaoDTOValidator());

            RuleFor(x => x.Favoritos)
                .Must(list => list == null || list.Count <= 5)
                .WithMessage("Você pode definir no máximo 5 jogos favoritos no pódio.")
                .Must(list => list == null || list.Select(f => f.Posicao).Distinct().Count() == list.Count)
                .WithMessage("Não podem haver posições repetidas no pódio de favoritos.")
                .Must(list => list == null || list.Select(f => f.JogoId).Distinct().Count() == list.Count)
                .WithMessage("Não podem haver jogos duplicados no pódio de favoritos.");
        }
    }
}
