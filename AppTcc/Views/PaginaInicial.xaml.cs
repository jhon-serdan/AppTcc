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
    private decimal _despesasFuturas = 0;

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

            var transacoesMesAtual = await App.DB.ListarTransacaoMes(mes, ano);


            decimal receitaMesAtual = 0;
            decimal despesaMesAtual = 0;

            foreach (var transacao in transacoesMesAtual)
            {
                if (transacao.Tipo == TipoTransacao.Receita)
                {
                    receitaMesAtual += transacao.Valor;
                }
                else if (transacao.Tipo == TipoTransacao.Despesa)
                {
                    despesaMesAtual += transacao.Valor;
                }
            }

            decimal saldoAcumuladoAnterior = await CalcularSaldoAcumuladoMesAnterior(mes, ano);

            _somarReceitas = receitaMesAtual;
            _somarDespesas = despesaMesAtual;
            _saldoTotal = saldoAcumuladoAnterior + receitaMesAtual - despesaMesAtual;

            await carregarDespesasFuturas(mes, ano);

            AtualizarInterface();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados: {ex.Message}");
            await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
        }
    }

    private async Task carregarDespesasFuturas(int mes, int ano)
    {
        try
        {
            var despesasFuturas = await App.DB.ListarDespesasFuturas(mes, ano);

            _despesasFuturas = 0;

            foreach (var despesa in despesasFuturas)
            {
                _despesasFuturas += despesa.Valor;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar despesas futuras: {ex.Message}");
        }
    }

    private async Task<decimal> CalcularSaldoAcumuladoMesAnterior (int mesSelecionado, int anoSelecionado)
    {
        decimal saldoAcumulado = 0;

        try
        {
            var dataLimite = new DateTime(anoSelecionado, mesSelecionado, 1).AddDays(-1);

            var transacoesAnteriores = await App.DB.ListarTransacaoAteData(dataLimite);

            foreach (var transacao in transacoesAnteriores)
            {
                if (transacao.Tipo == TipoTransacao.Receita)
                {
                    saldoAcumulado += transacao.Valor;
                }
                else if (transacao.Tipo == TipoTransacao.Despesa)
                {
                    saldoAcumulado -= transacao.Valor;
                }
            }
        } catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao calcular o saldo anterior: {ex.Message}");
        }

        return saldoAcumulado;
    }

    private void AtualizarInterface()
    {
        try
        {
            LblSaldoTotal.Text = _saldoTotal.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
            LblReceitaTotal.Text = _somarReceitas.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
            LblDespesaTotal.Text = _somarDespesas.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));

            LblDespesasFuturas.Text = _despesasFuturas.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
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
