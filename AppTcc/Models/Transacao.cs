using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTcc.Helper
{
    public class Transacao
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public int CategoriaId { get; set; }
        [Ignore]
        public string CategoriaNome { get; set; }
        public TipoTransacao Tipo { get; set; }
        public string Descricao { get; set; }
        public bool EParcelado { get; set; }
        public int? NumeroParcelas { get; set; }
        public int? ParcelaAtual { get; set; }
        public int? TransacaoOrigemId { get; set; }
        public string Conta { get; set; }
    }
}
