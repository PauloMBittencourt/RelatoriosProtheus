using RelatoriosProtheus.Models.Entities;

namespace RelatoriosProtheus.Models.ViewModels
{
    public class UsuarioViewModel
    {
        public int FuncionarioId { get; set; }
        public string Matricula { get; set; }
        public string Email { get; set; }
        public IQueryable<string> Perfis { get; set; }
        public List<Perfil>? PerfisLista { get; set; }        
        public IQueryable<string> Grupos { get; set; }
        public List<Grupo>? GrupoLista { get; set; }
    }
}
