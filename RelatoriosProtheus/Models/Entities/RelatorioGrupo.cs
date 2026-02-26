using RelatoriosProtheus.Migrations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RelatoriosProtheus.Models.Entities
{
    [Table("RelatorioGrupos")]
    public class RelatorioGrupo
    {
        [Key]
        [Column(Order = 0)]
        public int RelatoriosId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int GrupoId { get; set; }

        [ForeignKey(nameof(RelatoriosId))]
        public Relatorios RelatoriosFk { get; set; }
        [ForeignKey(nameof(GrupoId))]
        public Grupo GrupoFk { get; set; }
    }
}
