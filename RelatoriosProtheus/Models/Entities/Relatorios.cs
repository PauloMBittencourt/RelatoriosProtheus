using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RelatoriosProtheus.Models.Entities
{
    public class Relatorios
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public ICollection<Parametros> Parametros { get; set; }

        public ICollection<RelatorioGrupo> RelatorioGrupos { get; set; }
    }
}
