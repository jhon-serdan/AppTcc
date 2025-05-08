using AppTcc.Popups;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.PlatformConfiguration;
using AppTcc.Helper;
using System.Globalization;
using System.Collections.ObjectModel;

namespace AppTcc.Views;

public partial class PaginaInicial : ContentPage
{

    ObservableCollection<Transacao> lista = new ObservableCollection<Transacao>();

    private decimal _somarReceitas = 0;
    private decimal _somarDespesas = 0;
    private decimal _saldoTotal = 0;

    public PaginaInicial()
    {
        InitializeComponent();

        DatePickerPagInicial.Date = DateTime.Now;

        DatePickerPagInicial.DateSelected += OnDatePickerDateSelected;

        CarregarDadosMes(DateTime.Now.Month, DateTime.Now.Year);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        CarregarDadosMes(DatePickerPagInicial.Date.Month, DatePickerPagInicial.Date.Year);

    }

    private async void OnDatePickerDateSelected (object sender, DateChangedEventArgs e)
    {
        await CarregarDadosMes(e.NewDate.Month, e.NewDate.Year);
    }

    private async Task CarregarDadosMes (int mes, int ano)
    {
        try
        {
            var transacoes = await App.DB.ListarTransacaoMes(mes, ano);

            _somarReceitas = 0;
            _somarReceitas = 0;

            _somarReceitas = transacoes
                .Where(t => t.Tipo == TipoTransacao.Receita)
                .Sum(t => (decimal)t.Valor);

            _somarDespesas = transacoes
                .Where(t => t.Tipo == TipoTransacao.Despesa)
                .Sum(t => (decimal)t.Valor);

            _saldoTotal = _somarReceitas - _somarDespesas;

            AtualizarInterface();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar os dados {ex.Message}", "OK");
        }
    }

    private void AtualizarInterface ()
    {
        LblSaldoTotal.Text = _saldoTotal.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
        LblDespesaTotal.Text = _somarDespesas.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
        LblReceitaTotal.Text = _somarReceitas.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
    }

    private void FloatingActionButton_Clicked(object sender, EventArgs e)
    {
        var popup = new PopupAdd();
        this.ShowPopup(popup);
    }

    private async void ExportarBanco_Clicked(object sender, EventArgs e)
    {
        await ExportarBancoAsync();
    }

    public async Task ExportarBancoAsync()
    {
        try
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "banco_financas.db3");

            string exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "banco_financas.db3");

            File.Copy(dbPath, exportPath, true);

            await App.Current.MainPage.DisplayAlert("Sucesso", $"banco exportado para: {exportPath}", "OK");

        } catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Erro", $"Falha ao exportar: {ex.Message}", "OK");
        }
    }

}
