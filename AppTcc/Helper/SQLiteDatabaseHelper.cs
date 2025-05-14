using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SQLite;

namespace AppTcc.Helper
{
    public class SQLiteDatabaseHelper
    {
        private SQLiteAsyncConnection _conn;

        #region Conexão_SQLite e criação Tabelas
        public SQLiteDatabaseHelper(string dbPath)
        {
            _conn = new SQLiteAsyncConnection(dbPath);
            _conn.CreateTableAsync<Categoria>().Wait();
            _conn.CreateTableAsync<Transacao>().Wait();
            _conn.CreateTableAsync<Conta>().Wait();

            var categorias = _conn.Table<Categoria>().ToListAsync().Result;
            if (categorias.Count == 0)
            {
                InserirCatergoriasPadraoAsync();
            }
        }
        #endregion

        #region CRUD

        #region Criação das Categorias Padrao
        private void InserirCatergoriasPadraoAsync()
        {
            #region Categoria de Receita
            _conn.InsertAsync(new Categoria { Nome = "Salário", Tipo = TipoCategoria.Receita });
            _conn.InsertAsync(new Categoria { Nome = "Prêmio", Tipo = TipoCategoria.Receita });
            #endregion

            #region Categoria de Despesa
            _conn.InsertAsync(new Categoria { Nome = "Alimentação", Tipo = TipoCategoria.Despesa });
            _conn.InsertAsync(new Categoria { Nome = "Lazer", Tipo = TipoCategoria.Despesa });
            _conn.InsertAsync(new Categoria { Nome = "Assinatura", Tipo = TipoCategoria.Despesa });
            #endregion

            #region Categoria de Ambos
            _conn.InsertAsync(new Categoria { Nome = "Empréstimo", Tipo = TipoCategoria.Ambos });
            _conn.InsertAsync(new Categoria { Nome = "Investimento", Tipo = TipoCategoria.Ambos });
            _conn.InsertAsync(new Categoria { Nome = "Outros", Tipo = TipoCategoria.Ambos });
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

        public async Task<List<Transacao>> ListarTransacaoAsync()
        {
            var transacoes = await _conn.Table<Transacao>().OrderByDescending(t => t.Data).ToListAsync();
            var categorias = await _conn.Table<Categoria>().ToListAsync();

            foreach (var transacao in transacoes)
            {
                var categoria = categorias.FirstOrDefault(c => c.Id == transacao.CategoriaId);
                transacao.CategoriaNome = categoria?.Nome ?? "Categoria não encontrada";
            }

            return transacoes;

        }

        #endregion

        #region Listar Transação Por Mes

        public async Task<List<Transacao>> ListarTransacaoMes(int mes, int ano)
        {
            try
            {
                var primeiroDiaDoMes = new DateTime(ano, mes, 1);
                var ultimoDiaDoMes = primeiroDiaDoMes.AddMonths(1).AddDays(-1);

                var transacoes = await _conn.Table<Transacao>()
                    .Where(t => t.Data >= primeiroDiaDoMes && t.Data <= ultimoDiaDoMes)
                    .OrderByDescending(t => t.Data)
                    .ToListAsync();

                var categorias = await _conn.Table<Categoria>().ToListAsync();

                foreach (var transacao in transacoes)
                {
                    var categoria = categorias.FirstOrDefault(c => c.Id == transacao.CategoriaId);
                    transacao.CategoriaNome = categoria?.Nome ?? "Categoria não encontrada";
                }
                return transacoes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro em ListarTransacaoMes: {ex.Message}");
                return new List<Transacao>();
            }
        }

        #endregion

        #region Listar Transação Futuras

        public async Task<List<Transacao>> ListarDespesasFuturas(int mesReferencia, int anoReferencia)
        {
            try
            {
                
                var primeiroDiaProximoMes = new DateTime(anoReferencia, mesReferencia, 1).AddMonths(1);

                var transacoes = await _conn.Table<Transacao>()
                    .Where(t => t.Data >= primeiroDiaProximoMes && t.Tipo == TipoTransacao.Despesa)
                    .OrderBy(t => t.Data)
                    .ToListAsync();

                var categorias = await _conn.Table<Categoria>().ToListAsync();

                foreach (var transacao in transacoes)
                {
                    var categoria = categorias.FirstOrDefault(c => c.Id == transacao.CategoriaId);
                    transacao.CategoriaNome = categoria?.Nome ?? "Categoria não encontrada";
                }
                return transacoes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro em ListarDespesasFuturas: {ex.Message}");
                return new List<Transacao>();
            }
        }

        #endregion

        #region Salvar Transacao a Vista

        public Task<int> SalvarTransacoesAsync (Transacao transacao)
        {
            if (transacao.Id != 0)
            {
                return _conn.UpdateAsync(transacao);
            } 
            else
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

                transacao.Valor = valorParcela;
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

        #region Zerar banco transações

        public async Task LimparTabelaTransacoes()
        {
            var db = App.DB;

            var transacoes = await db.ListarTransacaoAsync();

            foreach (var transacao in transacoes)
            {
                await db.DeletarTransacao(transacao);
            }
        }


        #endregion

        #region Saldo Acumulado
        public async Task<List<Transacao>> ListarTransacaoAteData(DateTime dataLimite)
        {
            try
            {
                var transacoes = await _conn.Table<Transacao>()
                    .Where(t => t.Data <= dataLimite)
                    .OrderBy(t => t.Data)
                    .ToListAsync();

                var categorias = await _conn.Table<Categoria>().ToListAsync();

                foreach (var transacao in transacoes)
                {
                    var categoria = categorias.FirstOrDefault(c => c.Id == transacao.CategoriaId);
                    transacao.CategoriaNome = categoria?.Nome ?? "Categoria não encontrada";
                }
                return transacoes;
            } catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao ListarTransacaoAteData: {ex.Message}");
                return new List<Transacao>();
            }
        }

        #endregion

        #region Gerenciamento Contas

        #region Registra o movimento entre as contas
        public async Task<int> RegistrarMovimentacaoAsync(string tipo, decimal valor, string descricao, DateTime data, int transacaoId)
        {
            var movimento = new Conta()
            {
                Tipo = tipo,
                Valor = valor,
                Descricao = descricao,
                Data = data,
                TransacaoID = transacaoId
            };

            return await _conn.InsertAsync(movimento);
        }

        #endregion

        #region Calcular Saldo Conta
        public async Task<decimal> CalcularSaldoAsync(string tipo)
        {
            try
            {
                var saldo = await _conn.Table<Conta>()
                    .Where(c => c.Tipo == tipo)
                    .ToListAsync();

                return saldo.Sum(c => c.Valor);

            } catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao calcular o saldo: {ex.Message}");
                return 0;
            }
        }

        #endregion

        #region verificação se há saldo suficiente na conta

        public async Task<bool> TemSaldoSuficienteAsync(string tipo, decimal valorNecessario)
        {
            decimal saldo = await CalcularSaldoAsync(tipo);
            return saldo >= valorNecessario;
        }

        #endregion

        #endregion

        #region Processamento Trasanção Contas

        public async Task<int> ProcessarReceitaAsync (Transacao transacao)
        {
            int transacaoId = await _conn.InsertAsync(transacao);

            await RegistrarMovimentacaoAsync("Corrente", transacao.Valor, $"Receita: {transacao.Descricao}", transacao.Data, transacaoId);

            return transacaoId;
        } 

        public async Task<int> ProcessarDespesaAsync(Transacao transacao)
        {
            
            if (!await TemSaldoSuficienteAsync("Corrente", transacao.Valor))
                throw new Exception("Saldo insuficiente na conta corrente.");

            int transacaoId = await _conn.InsertAsync(transacao);

            await RegistrarMovimentacaoAsync("Corrente", -transacao.Valor, $"Despesa: {transacao.Descricao}", transacao.Data, transacaoId);

            return transacaoId;

        }

        private async Task<int> ProcessarTransferenciaAsync(Transacao transacao, string tipoOrigem, string tipoDestino)
        {
            if (!await TemSaldoSuficienteAsync(tipoOrigem, transacao.Valor))
                throw new Exception($"Saldo insuficiente na conta {tipoOrigem}.");

            int transacaoId = await _conn.InsertAsync(transacao);

            await RegistrarMovimentacaoAsync(tipoDestino, transacao.Valor, $"Transferência de {tipoOrigem}", transacao.Data, transacaoId);

            return transacaoId;
        }

        #endregion

        #endregion

    }
}
