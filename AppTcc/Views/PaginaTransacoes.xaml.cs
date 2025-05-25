using AppTcc.Helper;
using System.Collections.ObjectModel;

namespace AppTcc.Views
    {
    public partial class Transacoes : ContentPage
    {
        public ObservableCollection<Transacao> lista { get; set; }
        public Transacao ItemSelecionado { get; set; }

        public Transacoes()
        {
            InitializeComponent();
            lista = new ObservableCollection<Transacao>();
            BindingContext = this;

            // Carrega todas as transações inicialmente
            CarregarTransacoes();

            PckTransacoes.SelectedIndex = 0;
        }

        #region Método para carregar todas as transações
            private async void CarregarTransacoes()
            {
                try
                {
                    var transacoes = await App.DB.ListarTransacaoAsync();
                    AtualizarLista(transacoes);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", $"Erro ao carregar transações: {ex.Message}", "OK");
                }
            }

            #endregion

        #region Método para carregar transações por período
            private async void CarregarTransacoesPorPeriodo(DateTime dataEscolhida)
            {
                try
                {
                    int mes = dataEscolhida.Month;
                    int ano = dataEscolhida.Year;

                    var transacoes = await App.DB.ListarTransacaoMes(mes, ano);
                    AtualizarLista(transacoes);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", $"Erro ao carregar transações do período: {ex.Message}", "OK");
                }
            }

            #endregion

        #region Método para aparecer msg de sem transações
            private void AtualizarLista(List<Transacao> transacoes)
            {
                lista.Clear();

                if (transacoes != null && transacoes.Count > 0)
                {
                    foreach (var transacao in transacoes)
                    {
                        lista.Add(transacao);
                    }

                    // Mostra a lista e esconde a mensagem
                    lst_Transacoes.IsVisible = true;
                    LblSemTransacoes.IsVisible = false;
                }
                else
                {
                    // Esconde a lista e mostra a mensagem
                    lst_Transacoes.IsVisible = false;
                    LblSemTransacoes.IsVisible = true;
                }
            }

            #endregion

        #region Método para carregar transações por tipo de visualização
            private void PckTransacoes_SelectedIndexChanged(object sender, EventArgs e)
            {
                var picker = sender as Picker;

                if (picker.SelectedIndex == 0) // Geral
                {
                    GridPeriodo.IsVisible = false;
                    CarregarTransacoes(); // Carrega todas as transações
                }
                else if (picker.SelectedIndex == 1) // Período
                {
                    GridPeriodo.IsVisible = true;
                    // Carregar transações do mês atual como padrão
                    CarregarTransacoesPorPeriodo(DateTime.Now);
                }
            }
            #endregion

        #region Carregar transações por período
            private void DtPckTransacao_DateSelected(object sender, DateChangedEventArgs e)
            {
                // Quando uma nova data é selecionada, carrega as transações desse período
                CarregarTransacoesPorPeriodo(e.NewDate);
            }

        #endregion

        #region Método que ao clicar no item da collection, ele manda as informações por biding para edição
            private async void lst_Transacoes_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (e.CurrentSelection.FirstOrDefault() is Transacao transacaoSelecionada)
                    {
                    // Navegar para a página de editar passando a transação como parâmetro
                    await Shell.Current.GoToAsync($"{nameof(PaginaEditarItem)}",
                        new Dictionary<string, object>
                        {
                            {"TransacaoSelecionada", transacaoSelecionada}
                        });

                    // Limpar a seleção
                    ((CollectionView)sender).SelectedItem = null;
                }
            }
            #endregion

        #region Botão para zerar o banco de dados
        private async void Btn_LimparTransacao(object sender, EventArgs e)
            {
                try
                {
                    bool resposta = await DisplayAlert(
                        "Confirmação",
                        "Tem certeza que deseja excluir todas as transações? Esta ação não pode ser desfeita.",
                        "Sim",
                        "Cancelar");

                    if (resposta)
                    {
                        await App.DB.LimparTabelaTransacoes();

                        // Atualiza a lista (que ficará vazia)
                        AtualizarLista(new List<Transacao>());

                        await DisplayAlert("Sucesso", "Todas as transações foram excluídas.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Erro", $"Erro ao limpar transações: {ex.Message}", "OK");
                }
            }

            #endregion

        #region Método para atualizar a lista de transações
            protected async override void OnAppearing()
            {
                try
                {
                    lista.Clear();

                    List<Transacao> tmp = await App.DB.ListarTransacaoAsync();

                    tmp.ForEach(i => lista.Add(i));

                }
                catch (Exception ex)
                {
                    await DisplayAlert("Ops", ex.Message, "oK");
                }
            }

            #endregion
    }
}