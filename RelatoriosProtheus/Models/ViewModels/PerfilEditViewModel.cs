using System.ComponentModel.DataAnnotations;

namespace RelatoriosProtheus.Models.ViewModels
{
    public class PerfilEditViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Descricao { get; set; }
    }
}
