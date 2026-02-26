using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RelatoriosProtheus.Models.Entities
{
    public class Grupo
    {
        [Key]
        public int GrupoId { get; set; }
        [Required]
        public string Nome { get; set; }
        public string Cor { get; set; }
        public int GrupoSeparacao { get; set; }

        public ICollection<FuncionarioGrupo>? FuncionarioGrupos { get; set; } = new List<FuncionarioGrupo>();
        public ICollection<RelatorioGrupo>? RelatorioGrupos { get; set; } = new List<RelatorioGrupo>();
    }
}
