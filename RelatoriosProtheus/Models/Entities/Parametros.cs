using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RelatoriosProtheus.Models.Entities
{
    [Table("Parametros")]
    public class Parametros
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Param { get; set; }
        [Required]
        public string DescricaoEmTela { get; set; }
        [Required]
        public string Exemplo { get; set; }
        [Required]
        public string Tipo { get; set; }
        [Required]
        [DisplayName("Parâmetro Obrigatório")]
        public int Obrigatorio { get; set; }

        public int RelatorioId { get; set; }
        public Relatorios Relatorio { get; set; }
    }
}
