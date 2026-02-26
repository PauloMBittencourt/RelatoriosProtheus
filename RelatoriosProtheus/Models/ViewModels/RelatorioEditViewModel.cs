using RelatoriosProtheus.Models.Entities;
using System.Collections.Generic;

namespace RelatoriosProtheus.Models.ViewModels
{
    public class RelatorioEditViewModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public List<Parametros> Parametros { get; set; }
        public int? SelectedGrupoId { get; set; }
        public ICollection<RelatorioGrupo> RelatorioGrupos { get; set; }
    }
}
