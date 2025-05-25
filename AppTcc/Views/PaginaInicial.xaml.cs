using AppTcc.Popups;
using CommunityToolkit.Maui.Views;
using AppTcc.Helper;
using System.Globalization;
using Microcharts;
using SkiaSharp;

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

    #region M�todo para carregar as transa��es do m�s selecionado
    private async Task CarregarDadosMes(int mes, int ano)
    {
        try
        {

            _somarDespesas = 0;
            _somarReceitas = 0;
            _saldoTotal = 0;
            _despesasFuturas = 0;

            var transacoesMesAtual = await App.DB.ListarTransacaoMes(mes, ano);


            decimal receitaMesAtual = 0;
            decimal despesaMesAtual = 0;

            Dictionary<string, decimal> despesasPorCategoria = new Dictionary<string, decimal>();

            foreach (var transacao in transacoesMesAtual)
            {
                if (transacao.Tipo == TipoTransacao.Receita)
                {
                    receitaMesAtual += transacao.Valor;
                }
                else if (transacao.Tipo == TipoTransacao.Despesa)
                {
                    despesaMesAtual += transacao.Valor;

                    if (!despesasPorCategoria.ContainsKey(transacao.CategoriaNome))
                    {
                        despesasPorCategoria[transacao.CategoriaNome] = 0;
                    }
                    despesasPorCategoria[transacao.CategoriaNome] += transacao.Valor;
                }
            }

            decimal saldoAcumuladoAnterior = await CalcularSaldoAcumuladoMesAnterior(mes, ano);

            _somarReceitas = receitaMesAtual;
            _somarDespesas = despesaMesAtual;
            _saldoTotal = saldoAcumuladoAnterior + receitaMesAtual - despesaMesAtual;

            await carregarDespesasFuturas(mes, ano);

            CarregarDadosGrafico(despesasPorCategoria);

            AtualizarInterface();

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados: {ex.Message}");
            await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
        }
    }

    #endregion

    #region M�todo para carregar o gr�fico de despesas por categoria
    private void CarregarDadosGrafico(Dictionary<string, decimal> despesasPorCategoria)
    {

        DespesaGrafico.Chart = null; // Limpa o gr�fico anterior

        try
        {
            // Oculta o gr�fico se n�o houver dados
            if (despesasPorCategoria.Count == 0)
            {
                DespesaGrafico.Chart = null;
                DespesaGrafico.IsVisible = false;
                LblSemDespesa.IsVisible = true;
                return;
            }

            DespesaGrafico.IsVisible = true;
            LblSemDespesa.IsVisible = false;

            var cores = new List<SKColor>
        {
            SKColor.Parse("#FF5722"),  // Laranja
            SKColor.Parse("#3F51B5"),  // Azul
            SKColor.Parse("#4CAF50"),  // Verde
            SKColor.Parse("#9C27B0"),  // Roxo
            SKColor.Parse("#2196F3"),  // Azul claro
            SKColor.Parse("#FFC107"),  // Amarelo
            SKColor.Parse("#E91E63"),  // Rosa
            SKColor.Parse("#607D8B"),  // Cinza azulado
            SKColor.Parse("#795548"),  // Marrom
            SKColor.Parse("#009688")   // Esmeralda
        };

            var entradas = new List<ChartEntry>();
            int colorIndex = 0;

            foreach (var categoria in despesasPorCategoria.OrderByDescending(c => c.Value))
            {
                var cor = cores[colorIndex % cores.Count];
                colorIndex++;

                entradas.Add(new ChartEntry((float)categoria.Value)
                {
                    Label = categoria.Key,
                    ValueLabel = categoria.Value.ToString("C", CultureInfo.GetCultureInfo("pt-BR")),
                    Color = cor
                });
            }

            // Limpa o gr�fico atual
            DespesaGrafico.Chart = null;

            // For�a a atualiza��o com atraso e reinicializa��o do gr�fico
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(50);

                DespesaGrafico.Chart = new DonutChart
                {
                    Entries = entradas,
                    BackgroundColor = SKColors.Transparent,
                    HoleRadius = 0.5f,
                    LabelTextSize = 60f,
                    LabelMode = LabelMode.RightOnly,
                    GraphPosition = GraphPosition.AutoFill
                };

                DespesaGrafico.InvalidateMeasure(); 
            });

        } catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar o gr�fico: {ex.Message}");
        }
    }

    #endregion

    #region M�todo para carregar despesas futuras
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

    #endregion

    #region M�todo para calcular o saldo acumulado
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

    #endregion

    #region M�todo para colocar a moeda com o s�mbolo correto
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

    #endregion

    #region M�todo para abrir o popup de adicionar transa��o
    private void FloatingActionButton_Clicked(object sender, EventArgs e)
    {
        var popup = new PopupAdd();
        this.ShowPopup(popup);
    }

    #endregion

    #region M�todo para exportar o banco de dados
    private async void ExportarBanco_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Caminhos dos bancos de dados
            string appDir = "/data/user/0/com.companyname.apptcc/files/";
            string financasDbPath = Path.Combine(appDir, "banco_financas.db3");

            // Pasta de destino (Downloads � acess�vel ao usu�rio)
            string destinationDir = "/storage/emulated/0/Download/";

            if (File.Exists(financasDbPath))
            {
                File.Copy(financasDbPath, Path.Combine(destinationDir, "banco_financas_export.db3"), true);
            }

            await DisplayAlert("Sucesso", "Bancos de dados exportados para a pasta Downloads", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Falha ao exportar: {ex.Message}", "OK");
        }
    }

    #endregion
}
