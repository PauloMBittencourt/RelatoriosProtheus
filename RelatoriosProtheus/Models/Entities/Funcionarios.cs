using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RelatoriosProtheus.Models.Entities
{
    public class Funcionarios
    {
        [Key]
        public int Id { get; set; }
        public Empresa Empresa { get; set; }
        public string Matricula { get; set; }
        public string Senha { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public int Situacao { get; set; }
        public ICollection<FuncionarioGrupo> FuncionarioGrupos { get; set; }
        public List<Perfis_Funcionarios> Perfis { get; set; }
    }

    public enum Empresa : int
    {
        SENAI = 1,
        SESI = 3,
        IEL = 4,
        FIES = 5
    }
}
