using FluentValidation;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Validators
{
    public class CriarAvaliacaoDTOValidator : AbstractValidator<CriarAvaliacaoDTO>
    {
        public CriarAvaliacaoDTOValidator()
        {
            RuleFor(x => x.Nota)
                .InclusiveBetween(1, 5).WithMessage("A nota da avaliação deve estar entre 1 e 5 estrelas.");

            RuleFor(x => x.JogoId)
                .NotEmpty().WithMessage("O identificador do jogo é obrigatório.");

            RuleFor(x => x.TextoAvaliacao)
                .MaximumLength(1000).WithMessage("A análise pode ter no máximo 1000 caracteres.");
        }
    }

    public class EditarAvaliacaoDTOValidator : AbstractValidator<EditarAvaliacaoDTO>
    {
        public EditarAvaliacaoDTOValidator()
        {
            RuleFor(x => x.Nota)
                .InclusiveBetween(1, 5).WithMessage("A nota da avaliação deve estar entre 1 e 5 estrelas.");

            RuleFor(x => x.TextoAvaliacao)
                .MaximumLength(1000).WithMessage("A análise pode ter no máximo 1000 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.TextoAvaliacao));
        }
    }

    public class CriarRespostaDTOValidator : AbstractValidator<CriarRespostaDTO>
    {
        public CriarRespostaDTOValidator()
        {
            RuleFor(x => x.Comentario)
                .NotEmpty().WithMessage("O comentário não pode ser vazio.")
                .MaximumLength(500).WithMessage("O comentário pode ter no máximo 500 caracteres.");
        }
    }
}
