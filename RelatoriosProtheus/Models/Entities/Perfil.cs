using System.ComponentModel.DataAnnotations;

namespace RelatoriosProtheus.Models.Entities
{
    public class Perfil
    {
        [Key]
        public int Id { get; set; }
        public string Descricao { get; set; }

        public List<Perfis_Funcionarios> FuncionariosFk { get; set; }
    }
}
