using RelatoriosProtheus.Models.Entities;

namespace RelatoriosProtheus.Utils
{
    public class EnumsUtils
    {
        public static Empresa retornarEnum(string val)
        {
            return string.IsNullOrEmpty(val) ?
                Empresa.SENAI :
                (Empresa)Enum.Parse(typeof(Empresa), val);
        }
    }
}
