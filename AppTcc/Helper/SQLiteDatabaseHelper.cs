using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace AppTcc.Helper
{
    public class SQLiteDatabaseHelper
    {
        private SQLiteAsyncConnection _conn;

        #region Conexão_SQLite e criação Tabelas
        public SQLiteDatabaseHelper(string dbPath)
            {
                _conn = new SQLiteAsyncConnection(dbPath); ;
            }
        public async Task InitializeDatabase()
        {
            if (_conn != null)
            {
                await _conn.CreateTableAsync<Categoria>();
                await _conn.CreateTableAsync<Transacao>();

                var catergoria = await _conn.Table<Categoria>().ToListAsync(); ;
                if (catergoria.Count == 0)
                {
                    await InserirCatergoriasPadraoAsync();
                }
            }            
        }
        #endregion

        #region CRUD

        #region Criação das Categorias Padrao
        private async Task InserirCatergoriasPadraoAsync()
        {
            #region Categoria de Receita
            await _conn.InsertAsync(new Categoria { Nome = "Salário", Tipo = TipoCategoria.Receita });
            await _conn.InsertAsync(new Categoria { Nome = "Prêmio", Tipo = TipoCategoria.Receita });
            #endregion

            #region Categoria de Despesa
            await _conn.InsertAsync(new Categoria { Nome = "Alimentação", Tipo = TipoCategoria.Despesa });
            await _conn.InsertAsync(new Categoria { Nome = "Lazer", Tipo = TipoCategoria.Despesa });
            await _conn.InsertAsync(new Categoria { Nome = "Assinatura", Tipo = TipoCategoria.Despesa });
            #endregion

            #region Categoria de Ambos
            await _conn.InsertAsync(new Categoria { Nome = "Empréstimo", Tipo = TipoCategoria.Ambos });
            await _conn.InsertAsync(new Categoria { Nome = "Investimento", Tipo = TipoCategoria.Ambos });
            await _conn.InsertAsync(new Categoria { Nome = "Outros", Tipo = TipoCategoria.Ambos });
            #endregion
        }
        #endregion

        #region Listar Categorias

        public Task<List<Categoria>> ListaCategoria (TipoCategoria? tipo = null)
        {
            if (tipo.HasValue)
            {
                return _conn.Table<Categoria>()
                    .Where(c => c.Tipo == tipo.Value || c.Tipo == TipoCategoria.Ambos)
                    .ToListAsync();
            }
            return _conn.Table<Categoria>().ToListAsync();
        }

        #endregion

        #region Listar Todas Transacoes

        public Task<List<Transacao>> ListarTransacaoAsync ()
        {
                return _conn.Table<Transacao>().OrderByDescending(t => t.Data).ToListAsync();
        }

        #endregion

        #region Listar Transação Por Mes

        public Task<List<Transacao>> ListarTransacaoMes (int mes, int ano)
        {
            return _conn.Table<Transacao>()
                .Where(t => t.Data.Month == mes && t.Data.Year == ano)
                .OrderByDescending(t => t.Data)
                .ToListAsync();
        }

        #endregion

        #region Salvar Transacao a Vista

        public Task<int> SalvarTransacoesAsync (Transacao transacao)
        {
            if (transacao.Id != 0)
            {
                return _conn.UpdateAsync(transacao);
            } else
            {
                return _conn.InsertAsync(transacao);
            }
        }

        #endregion

        #region Salvar Transacao Parcelada

        public async Task<List<int>> SalvarTransacaoParcelada (Transacao transacao)
        {
            var ids = new List<int>();

            if (transacao.EParcelado && transacao.NumeroParcelas.HasValue &&  transacao.NumeroParcelas > 1)
            {
                decimal valorParcela = Math.Round(transacao.Valor / transacao.NumeroParcelas.Value, 2);

                transacao.ParcelaAtual = 1;
                int id = await _conn.InsertAsync(transacao);
                ids.Add(id);

                for (int i = 2; i <= transacao.NumeroParcelas.Value; i++)
                {
                    var novaParcela = new Transacao
                    {
                        Valor = valorParcela,
                        Data = transacao.Data.AddMonths(i - 1),
                        CategoriaId = transacao.CategoriaId,
                        Tipo = transacao.Tipo,
                        Descricao = transacao.Descricao,
                        EParcelado = true,
                        NumeroParcelas = transacao.NumeroParcelas,
                        ParcelaAtual = i,
                        TransacaoOrigemId = transacao.Id,
                        Conta = transacao.Conta
                    };

                    int novoID = await _conn.InsertAsync(novaParcela);
                    ids.Add(novoID);
                }
            }
            else
            {
                int id = await _conn.InsertAsync(transacao);
                ids.Add(id);
            }
            return ids;
        }

        #endregion

        #region Deletar Transacao

        public Task<int> DeletarTransacao (Transacao transacao)
        {
            return _conn.DeleteAsync(transacao);
        }

        #endregion

        #endregion

    }
}
