using System.ComponentModel.DataAnnotations;

namespace RelatoriosProtheus.Models.ViewModels
{
    public class PerfilCreateViewModel
    {
        [Required]
        public string Descricao { get; set; }
    }
}
