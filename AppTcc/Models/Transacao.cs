﻿using SQLite;
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
        public string CategoriaNome { get; set; } = string.Empty;
        public TipoTransacao Tipo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public bool EParcelado { get; set; }
        public int? NumeroParcelas { get; set; }
        public int? ParcelaAtual { get; set; }
        public int? TransacaoOrigemId { get; set; }
        public string Conta { get; set; } = string.Empty;

        [Ignore]
        public Color CorValor
        {
            get
            {
                return Tipo == TipoTransacao.Receita ? Colors.Green : Colors.Red;
            }
        }
    }
}
