using RelatoriosProtheus.Migrations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RelatoriosProtheus.Models.Entities
{
    [Table("FuncionarioGrupos")]
    public class FuncionarioGrupo
    {
        [Key]
        [Column(Order = 0)]
        public int GrupoId { get; set; }
        [Key]
        [Column(Order = 1)]
        public int FuncionariosId { get; set; }

        [ForeignKey(nameof(GrupoId))]
        public Grupo GrupoFk { get; set; }
        [ForeignKey(nameof(FuncionariosId))]
        public Funcionarios FuncionarioFk { get; set; }
    }
}
