using RelatoriosProtheus.Models.Entities;

namespace RelatoriosProtheus.Models.ViewModels
{
    public class RelatorioFiltroViewModel
    {
        public int? GrupoSelecionadoId { get; set; }
        public string? DescricaoRelatorio { get; set; }
        public bool ModalParametros { get; set; }
        public int ModalId { get; set; }
        public string ModalAcao { get; set; }
        public int? TipoArquivo { get; set; }

        public List<Grupo> GruposDisponiveis { get; set; } = new List<Grupo>();
        public List<Relatorios> Relatorios { get; set; } = new List<Relatorios>();
    }
}
