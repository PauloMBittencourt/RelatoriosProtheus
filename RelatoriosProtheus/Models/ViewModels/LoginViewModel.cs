using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RelatoriosProtheus.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        public string Empresa { get; set; }
        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        public string Matricula { get; set; }
        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        public string Senha { get; set; }
        [DisplayName("Lembrar de Mim?")]
        public bool RememberMe { get; set; }
        [ValidateNever]
        public string ReturnUrl { get; set; }
    }
}
