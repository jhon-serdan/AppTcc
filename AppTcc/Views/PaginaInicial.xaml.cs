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

        CarregarDadosMes(DatePickerPagInicial.Date.Month, DatePickerPagInicial.Date.Year);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        CarregarDadosMes(DatePickerPagInicial.Date.Month, DatePickerPagInicial.Date.Year);

    }

    private void OnDatePickerDateSelected (object sender, DateChangedEventArgs e)
    {
        CarregarDadosMes(e.NewDate.Month, e.NewDate.Year);
    }

    private async Task CarregarDadosMes(int mes, int ano)
    {
        try
        {

            var transacoes = await App.DB.ListarTransacaoMes(mes, ano);


            _somarReceitas = 0;
            _somarDespesas = 0;

            foreach (var transacao in transacoes)
            {
                if (transacao.Tipo == TipoTransacao.Receita)
                {
                    _somarReceitas += transacao.Valor;
                }
                else if (transacao.Tipo == TipoTransacao.Despesa)
                {
                    _somarDespesas += transacao.Valor;
                }
            }


            _saldoTotal = _somarReceitas - _somarDespesas;


            AtualizarInterface();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados: {ex.Message}");
            await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
        }
    }

    private void AtualizarInterface()
    {
        try
        {
            LblSaldoTotal.Text = _saldoTotal.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
            LblReceitaTotal.Text = _somarReceitas.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
            LblDespesaTotal.Text = _somarDespesas.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao atualizar interface: {ex.Message}");
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
