using GameLog_Backend.Database;
using GameLog_Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GameLog_Backend.Services
{
    public class JogoServices
    {
        protected readonly GameLogContext context;

        public JogoServices(GameLogContext context)
        {
            this.context = context;
        }

        public IEnumerable<JogoDTO> ListarJogos()
        {
            var jogos = context.Jogos
                .Include(j => j.Generos) 
                .Include(j => j.Empresa)  
                .Select(g => new JogoDTO
                {
                    JogoId = g.Id,
                    Titulo = g.Titulo,
                    Descricao = g.Descricao,
                    Imagem = g.Imagem,
                    DataLancamento = g.DataLancamento,
                    ClassificacaoIndicativa = g.ClassificacaoIndicativa,
                    EmpresaId = g.Empresa.Id,
                    EstaAtivo = g.EstaAtivo
                }).ToList();

            return jogos;
        }
    }
}