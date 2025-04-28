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

    }
}
