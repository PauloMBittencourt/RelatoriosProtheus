using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace RelatoriosProtheus.Models.ViewModels
{
    public class EsqueceuSenhaViewModel
    {
        [ValidateNever]
        public string Nome { get; set; }
        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        public string Email { get; set; }
        [ValidateNever]
        public string Matricula { get; set; }
        [ValidateNever]
        public string Senha { get; set; }
    }
}
