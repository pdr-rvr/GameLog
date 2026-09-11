using FluentValidation;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Validators
{
    public class CriarListaDTOValidator : AbstractValidator<CriarListaDTO>
    {
        public CriarListaDTOValidator()
        {
            RuleFor(x => x.Titulo)
                .NotEmpty().WithMessage("O título da coleção é obrigatório.")
                .MinimumLength(2).WithMessage("O título deve ter pelo menos 2 caracteres.")
                .MaximumLength(100).WithMessage("O título pode ter no máximo 100 caracteres.");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("A descrição pode ter no máximo 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Descricao));
        }
    }

    public class EditarListaDTOValidator : AbstractValidator<EditarListaDTO>
    {
        public EditarListaDTOValidator()
        {
            RuleFor(x => x.Titulo)
                .NotEmpty().WithMessage("O título da coleção é obrigatório.")
                .MinimumLength(2).WithMessage("O título deve ter pelo menos 2 caracteres.")
                .MaximumLength(100).WithMessage("O título pode ter no máximo 100 caracteres.");

            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("A descrição pode ter no máximo 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Descricao));
        }
    }

    public class AdicionarJogoListaDTOValidator : AbstractValidator<AdicionarJogoListaDTO>
    {
        public AdicionarJogoListaDTOValidator()
        {
            RuleFor(x => x.JogoId)
                .NotEmpty().WithMessage("O identificador do jogo é obrigatório.");
        }
    }
}
