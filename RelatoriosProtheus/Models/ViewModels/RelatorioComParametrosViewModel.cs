using RelatoriosProtheus.Models.Entities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RelatoriosProtheus.Models.ViewModels
{
    public class RelatorioComParametrosViewModel
    {
        public int RelatorioId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public List<ParametrosViewModel> Parametros { get; set; }
        public string Acao { get; set; }
        public int? TipoArquivo { get; set; }
    }

    public class ParametrosViewModel
    {
        public int? Id { get; set; }
        public string? Param { get; set; }
        public string? DescricaoEmTela { get; set; }
        public string? Exemplo { get; set; }
        public string? Tipo { get; set; }
        public string? Valor { get; set; }

        [DisplayName("Parâmetro Obrigatório")]
        public int Obrigatorio { get; set; } = 0;
    }
}
