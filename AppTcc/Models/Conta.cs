using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace AppTcc.Helper
{
    public class Conta
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Tipo {get; set; }
        public decimal Valor { get; set; }
        public int CategoriaId { get; set; }
        public string CategoriaNome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public DateTime Data { get; set; }
        public int TransacaoID { get; set; }

    }
}
