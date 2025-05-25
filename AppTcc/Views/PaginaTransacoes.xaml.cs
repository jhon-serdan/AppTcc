using AppTcc.Helper;
using AppTcc.Popups;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;

namespace AppTcc.Views;

public partial class Transacoes : ContentPage
{
    public ObservableCollection<Transacao> lista { get; set; } = new ObservableCollection<Transacao>();
    public Transacao ItemSelecionado { get; set; }

    public Transacoes()
    {
        InitializeComponent();

        BindingContext = this;

        // Define "Geral" como opção padrão
        PckTransacoes.SelectedIndex = 0;

        // Carrega todas as transações inicialmente
        _ = CarregarTransacoes();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Verifica qual modo está selecionado e carrega os dados apropriados
        if (PckTransacoes.SelectedIndex == 0) // Geral
        {
            await CarregarTransacoes();
        }
        else if (PckTransacoes.SelectedIndex == 1) // Período
        {
            // Garante que o grid está visível
            GridPeriodo.IsVisible = true;

            // Se não tem data definida, usa a atual
            if (DtPckTransacao.Date == default(DateTime) || DtPckTransacao.Date.Year < 2000)
            {
                DtPckTransacao.Date = DateTime.Now;
            }

            // Carrega as transações do período selecionado
            var data = DtPckTransacao.Date;
            await CarregarTransacoesPorPeriodo(data.Month, data.Year);
        }
    }

    private async Task CarregarTransacoes()
    {
        try
        {
            if (App.DB == null)
            {
                await DisplayAlert("Erro", "Banco de dados não inicializado", "OK");
                return;
            }

            var transacoes = await App.DB.ListarTransacaoAsync();

            // Usa Dispatcher para operações na UI
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                lista.Clear();
                if (transacoes != null)
                {
                    foreach (var transacao in transacoes)
                    {
                        lista.Add(transacao);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar transações: {ex.Message}", "OK");
        }
    }

    private async Task CarregarTransacoesPorPeriodo(int mes, int ano)
    {
        try
        {
            if (App.DB == null)
            {
                await DisplayAlert("Erro", "Banco de dados não inicializado", "OK");
                return;
            }

            var transacoes = await App.DB.ListarTransacaoMes(mes, ano);

            // Usa MainThread para operações na UI
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                lista.Clear();
                if (transacoes != null)
                {
                    foreach (var transacao in transacoes)
                    {
                        lista.Add(transacao);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar transações do período: {ex.Message}\nMês: {mes}, Ano: {ano}", "OK");
        }
    }

    private async void PckTransacoes_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            var picker = sender as Picker;

            if (picker == null) return;

            if (picker.SelectedIndex == 0) // Geral
            {
                GridPeriodo.IsVisible = false;
                await CarregarTransacoes();
            }
            else if (picker.SelectedIndex == 1) // Período
            {
                GridPeriodo.IsVisible = true;

                // Define a data atual no DatePicker se ainda não foi definida
                if (DtPckTransacao.Date == default(DateTime) || DtPckTransacao.Date.Year < 2000)
                {
                    DtPckTransacao.Date = DateTime.Now;
                }

                // Carrega as transações do mês/ano atual
                var data = DtPckTransacao.Date;
                await CarregarTransacoesPorPeriodo(data.Month, data.Year);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao alterar filtro: {ex.Message}", "OK");
        }
    }

    private async void DtPckTransacao_DateSelected(object sender, DateChangedEventArgs e)
    {
        try
        {
            if (PckTransacoes.SelectedIndex == 1 && e?.NewDate != null) // Só executa se estiver no modo "Período"
            {
                var data = e.NewDate;

                // Validação da data
                if (data.Year >= 1900 && data.Year <= 2100)
                {
                    await CarregarTransacoesPorPeriodo(data.Month, data.Year);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao filtrar por data: {ex.Message}", "OK");
        }
    }

    private async void lst_Transacoes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e?.CurrentSelection?.FirstOrDefault() is Transacao transacaoSelecionada)
            {
                ItemSelecionado = transacaoSelecionada;

                var popup = new PopupDetalheItem();
                await this.ShowPopupAsync(popup);

                // Limpa a seleção
                if (sender is CollectionView collectionView)
                {
                    collectionView.SelectedItem = null;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao selecionar transação: {ex.Message}", "OK");
        }
    }

    private async void Btn_LimparTransacao(object sender, EventArgs e)
    {
        try
        {
            bool resposta = await DisplayAlert("Confirmação",
                "Tem certeza que deseja apagar todas as transações? Esta ação não pode ser desfeita.",
                "Sim", "Não");

            if (resposta)
            {
                if (App.DB == null)
                {
                    await DisplayAlert("Erro", "Banco de dados não inicializado", "OK");
                    return;
                }

                await App.DB.LimparTabelaTransacoes();
                await DisplayAlert("Sucesso", "Todas as transações foram removidas.", "OK");

                // Recarrega a lista baseado no filtro atual
                if (PckTransacoes.SelectedIndex == 0)
                {
                    await CarregarTransacoes();
                }
                else if (PckTransacoes.SelectedIndex == 1)
                {
                    var data = DtPckTransacao.Date;
                    await CarregarTransacoesPorPeriodo(data.Month, data.Year);
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao limpar transações: {ex.Message}", "OK");
        }
    }
}