using System.ComponentModel.DataAnnotations;

namespace RelatoriosProtheus.Models.ViewModels
{
    public class CreateRelatoriosViewModel
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        public string Nome { get; set; }
        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        public string Descricao { get; set; }
        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        public List<ParametroItemViewModel> Items { get; set; } = new();
    }

    public class ParametroItemViewModel
    {

        [Required(ErrorMessage = "Os parametros são obrigatório!")] public string Param { get; set; }
        [Required(ErrorMessage = "O campo {0} é obrigatório!")] public string DescricaoEmTela { get; set; }
        [Required(ErrorMessage = "O campo {0} é obrigatório!")] public string Exemplo { get; set; }
        [Required] public string Tipo { get; set; }
        public int Obrigatorio { get; set; } = 0;
    }

}
