using RelatoriosProtheus.Models.Entities;

namespace RelatoriosProtheus.Models.ViewModels
{
    public class FuncionarioEditViewModel
    {
        public int Id { get; set; }
        public Empresa Empresa { get; set; }
        public string Matricula { get; set; }
        public string Senha { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public bool Situacao { get; set; }
    }
}
