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
        public string Descricao { get; set; }
        public DateTime Data { get; set; }
        public int TransacaoID { get; set; }

    }
}
