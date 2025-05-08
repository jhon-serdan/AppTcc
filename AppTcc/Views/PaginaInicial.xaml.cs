using AppTcc.Popups;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls.PlatformConfiguration;
using AppTcc.Helper;
using System.Globalization;
using System.Collections.ObjectModel;

namespace AppTcc.Views;

public partial class PaginaInicial : ContentPage
{

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

        CarregarDadosMes(DateTime.Now.Month, DateTime.Now.Year);

    }

    private void OnDatePickerDateSelected (object sender, DateChangedEventArgs args)
    {
        CarregarDadosMes(args.NewDate.Month, args.NewDate.Year);
    }

    private async void CarregarDadosMes (int mes, int ano)
    {
        try
        {

            _somarReceitas = 0;
            _somarDespesas = 0;
            _saldoTotal = 0;

            LblSaldoTotal.Text = _saldoTotal.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
            LblReceitaTotal.Text = _somarReceitas.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
            LblDespesaTotal.Text = _somarDespesas.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));

            if (App.DB == null)
            {
                await DisplayAlert("Erro", "Banco de dados não inicializado", "OK");
                return;
            }


            var transacoes = await App.DB.ListarTransacaoMes(mes, ano);

            if (transacoes == null || !transacoes.Any())
            {
                return;
            }

            var receitas = transacoes.Where(t => t.Tipo == TipoTransacao.Receita).ToList();
            if (receitas.Any())
            {
                _somarReceitas = receitas.Sum(t => t.Valor);
            }

            var despesas = transacoes.Where(t => t.Tipo == TipoTransacao.Despesa).ToList();
            if (receitas.Any())
            {
                _somarDespesas = despesas.Sum(t => t.Valor);
            }

            _saldoTotal = _somarReceitas - _somarDespesas;

            LblSaldoTotal.Text = _saldoTotal.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
            LblReceitaTotal.Text = _somarReceitas.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
            LblDespesaTotal.Text = _somarDespesas.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));



        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar os dados teste {ex.Message}", "OK");
        }
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
