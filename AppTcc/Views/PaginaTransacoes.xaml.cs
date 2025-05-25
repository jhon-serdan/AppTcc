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

        // Define "Geral" como op��o padr�o
        PckTransacoes.SelectedIndex = 0;

        // Carrega todas as transa��es inicialmente
        _ = CarregarTransacoes();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Verifica qual modo est� selecionado e carrega os dados apropriados
        if (PckTransacoes.SelectedIndex == 0) // Geral
        {
            await CarregarTransacoes();
        }
        else if (PckTransacoes.SelectedIndex == 1) // Per�odo
        {
            // Garante que o grid est� vis�vel
            GridPeriodo.IsVisible = true;

            // Se n�o tem data definida, usa a atual
            if (DtPckTransacao.Date == default(DateTime) || DtPckTransacao.Date.Year < 2000)
            {
                DtPckTransacao.Date = DateTime.Now;
            }

            // Carrega as transa��es do per�odo selecionado
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
                await DisplayAlert("Erro", "Banco de dados n�o inicializado", "OK");
                return;
            }

            var transacoes = await App.DB.ListarTransacaoAsync();

            // Usa Dispatcher para opera��es na UI
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
            await DisplayAlert("Erro", $"Erro ao carregar transa��es: {ex.Message}", "OK");
        }
    }

    private async Task CarregarTransacoesPorPeriodo(int mes, int ano)
    {
        try
        {
            if (App.DB == null)
            {
                await DisplayAlert("Erro", "Banco de dados n�o inicializado", "OK");
                return;
            }

            var transacoes = await App.DB.ListarTransacaoMes(mes, ano);

            // Usa MainThread para opera��es na UI
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
            await DisplayAlert("Erro", $"Erro ao carregar transa��es do per�odo: {ex.Message}\nM�s: {mes}, Ano: {ano}", "OK");
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
            else if (picker.SelectedIndex == 1) // Per�odo
            {
                GridPeriodo.IsVisible = true;

                // Define a data atual no DatePicker se ainda n�o foi definida
                if (DtPckTransacao.Date == default(DateTime) || DtPckTransacao.Date.Year < 2000)
                {
                    DtPckTransacao.Date = DateTime.Now;
                }

                // Carrega as transa��es do m�s/ano atual
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
            if (PckTransacoes.SelectedIndex == 1 && e?.NewDate != null) // S� executa se estiver no modo "Per�odo"
            {
                var data = e.NewDate;

                // Valida��o da data
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

                // Limpa a sele��o
                if (sender is CollectionView collectionView)
                {
                    collectionView.SelectedItem = null;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao selecionar transa��o: {ex.Message}", "OK");
        }
    }

    private async void Btn_LimparTransacao(object sender, EventArgs e)
    {
        try
        {
            bool resposta = await DisplayAlert("Confirma��o",
                "Tem certeza que deseja apagar todas as transa��es? Esta a��o n�o pode ser desfeita.",
                "Sim", "N�o");

            if (resposta)
            {
                if (App.DB == null)
                {
                    await DisplayAlert("Erro", "Banco de dados n�o inicializado", "OK");
                    return;
                }

                await App.DB.LimparTabelaTransacoes();
                await DisplayAlert("Sucesso", "Todas as transa��es foram removidas.", "OK");

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
            await DisplayAlert("Erro", $"Erro ao limpar transa��es: {ex.Message}", "OK");
        }
    }
}