using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RelatoriosProtheus.Models.Entities
{
    public class Perfis_Funcionarios
    {
        [Key]
        [Column(Order = 0)]
        public int PerfilId { get; set; }
        [Key]
        [Column(Order = 1)]
        public int FuncionarioId { get; set; }

        [ForeignKey(nameof(PerfilId))]
        public Perfil PerfilFk { get; set; }

        [ForeignKey(nameof(FuncionarioId))]
        public Funcionarios FuncionarioFk { get; set; }
    }
}
