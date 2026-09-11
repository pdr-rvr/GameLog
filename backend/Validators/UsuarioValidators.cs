using System.Text.RegularExpressions;
using FluentValidation;
using GameLog_Backend.DTOs;

namespace GameLog_Backend.Validators
{
    public class CriarUsuarioDTOValidator : AbstractValidator<CriarUsuarioDTO>
    {
        public CriarUsuarioDTOValidator()
        {
            RuleFor(x => x.NomeUsuario)
                .NotEmpty().WithMessage("O nome de usuário é obrigatório.")
                .MinimumLength(3).WithMessage("O nome de usuário deve ter pelo menos 3 caracteres.")
                .MaximumLength(30).WithMessage("O nome de usuário pode ter no máximo 30 caracteres.")
                .Matches(@"^[a-zA-Z0-9_.-]+$").WithMessage("O nome de usuário só pode conter letras, números, sublinhados, pontos ou traços.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("O formato do e-mail informado é inválido.")
                .MaximumLength(100).WithMessage("O e-mail pode ter no máximo 100 caracteres.");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(6).WithMessage("A senha deve conter no mínimo 6 caracteres.")
                .MaximumLength(100).WithMessage("A senha pode ter no máximo 100 caracteres.");

            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("A bio pode ter no máximo 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Bio));
        }
    }

    public class UsuarioLoginDTOValidator : AbstractValidator<UsuarioLoginDTO>
    {
        public UsuarioLoginDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("O formato do e-mail informado é inválido.");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("A senha é obrigatória.");
        }
    }

    public class EditarUsuarioDTOValidator : AbstractValidator<EditarUsuarioDTO>
    {
        public EditarUsuarioDTOValidator()
        {
            RuleFor(x => x.NomeUsuario)
                .NotEmpty().WithMessage("O nome de usuário é obrigatório.")
                .MinimumLength(3).WithMessage("O nome de usuário deve ter pelo menos 3 caracteres.")
                .MaximumLength(30).WithMessage("O nome de usuário pode ter no máximo 30 caracteres.")
                .Matches(@"^[a-zA-Z0-9_.-]+$").WithMessage("O nome de usuário só pode conter letras, números, sublinhados, pontos ou traços.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("O formato do e-mail informado é inválido.")
                .MaximumLength(100).WithMessage("O e-mail pode ter no máximo 100 caracteres.");

            RuleFor(x => x.SenhaAtual)
                .NotEmpty().WithMessage("A senha atual é obrigatória para salvar as alterações.");

            RuleFor(x => x.NovaSenha)
                .MinimumLength(6).WithMessage("A nova senha deve ter no mínimo 6 caracteres.")
                .MaximumLength(100).WithMessage("A nova senha pode ter no máximo 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.NovaSenha));

            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("A bio pode ter no máximo 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Bio));
        }
    }

    public class DeletarUsuarioDTOValidator : AbstractValidator<DeletarUsuarioDTO>
    {
        public DeletarUsuarioDTOValidator()
        {
            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("A senha é obrigatória para confirmar a exclusão da conta.");
        }
    }
}
