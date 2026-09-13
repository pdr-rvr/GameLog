using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace GameLog_Backend.Entities
{
    public enum StatusJogo
    {
        [Display(Name = "Quero Jogar")]
        QueroJogar = 1,

        [Display(Name = "Jogando")]
        Jogando = 2,

        [Display(Name = "Zerado")]
        Zerado = 3,

        [Display(Name = "Pausado")]
        Pausado = 4,

        [Display(Name = "Abandonado")]
        Abandonado = 5
    }

    public static class StatusJogoExtensions
    {
        public static string ObterNomeExibicao(this StatusJogo status)
        {
            var member = typeof(StatusJogo).GetMember(status.ToString());
            if (member.Length > 0)
            {
                var display = member[0].GetCustomAttribute<DisplayAttribute>();
                if (display != null && !string.IsNullOrEmpty(display.Name))
                {
                    return display.Name;
                }
            }
            return status.ToString();
        }
    }

    public class BibliotecaJogo : Entity<Guid>
    {
        public Guid UsuarioId { get; set; }
        public virtual Usuario Usuario { get; set; } = null!;

        public Guid JogoId { get; set; }
        public virtual Jogo Jogo { get; set; } = null!;

        public StatusJogo Status { get; set; }

        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataConclusao { get; set; }
    }
}
