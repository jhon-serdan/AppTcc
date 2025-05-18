using AppTcc.Popups;
using CommunityToolkit.Maui.Views;
using AppTcc.Helper;
using System.Globalization;
using Microcharts;
using SkiaSharp;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Text;

namespace AppTcc.Views;

public partial class PaginaInicial : ContentPage
{

    private decimal _somarReceitas = 0;
    private decimal _somarDespesas = 0;
    private decimal _saldoTotal = 0;
    private decimal _despesasFuturas = 0;
    private decimal _saldoPoupanca = 0;
    private decimal _saldoCorrente = 0;
    private decimal _saldoTotal2 = 0;

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

    private void CarregarDadosGrafico(Dictionary<string, decimal> despesasPorCategoria)
    {

        DespesaGrafico.Chart = null; // Limpa o gráfico anterior

        try
        {
            // Oculta o gráfico se não houver dados
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

            // Limpa o gráfico atual
            DespesaGrafico.Chart = null;

            // Força o "refresh" com atraso e reinicialização do gráfico
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(50); // pequeno atraso para garantir que o gráfico anterior seja removido

                DespesaGrafico.Chart = new DonutChart
                {
                    Entries = entradas,
                    BackgroundColor = SKColors.Transparent,
                    HoleRadius = 0.5f,
                    LabelTextSize = 60f,
                    LabelMode = LabelMode.RightOnly,
                    GraphPosition = GraphPosition.AutoFill
                };

                DespesaGrafico.InvalidateMeasure(); // atualiza visualmente
            });

        } catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar o gráfico: {ex.Message}");
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

            LblSaldoCarteira.Text = _saldoCorrente.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
            LblSaldoPoupanca.Text = _saldoPoupanca.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));
            LblSaldoTotal2.Text = _saldoTotal2.ToString("C", CultureInfo.GetCultureInfo("pt-BR"));

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

    public async Task CarregarSaldos()
    {
        try
        {
            var db = App.DB;

            decimal _saldoCorrente = await db.CalcularSaldoAsync("Corrente");
            decimal _saldoPoupanca = await db.CalcularSaldoAsync("Poupança");
            decimal _saldoTotal2 = _saldoCorrente + _saldoPoupanca;




        } catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao carregar saldos: {ex.Message}");
        }
    }

    private async void ExportarBanco_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Caminhos dos bancos de dados (conforme visto na sua screenshot)
            string appDir = "/data/user/0/com.companyname.apptcc/files/";
            string financasDbPath = Path.Combine(appDir, "banco_financas.db3");

            // Pasta de destino (Downloads é acessível ao usuário)
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

    public async Task ExportarBancoAsync()
    {
        try
        {
            // Caminho de ORIGEM: Diretório de dados interno do aplicativo
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "banco_financas.db3");

            // Caminho de DESTINO: Pasta de Downloads
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string exportPath = Path.Combine(downloadsPath, "banco_financas.db3");

            await App.Current.MainPage.DisplayAlert("Caminho de Origem (DB):", dbPath, "OK"); // Para verificar
            await App.Current.MainPage.DisplayAlert("Caminho de Destino (Downloads):", exportPath, "OK"); // Para verificar

            File.Copy(dbPath, exportPath, true);

            await App.Current.MainPage.DisplayAlert("Sucesso", $"banco exportado para: {exportPath}", "OK");

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Erro", $"Falha ao exportar: {ex.Message}", "OK");
        }
    }

    private async void ListarDiretorios_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Começamos com o diretório base da aplicação
            string baseDir = FileSystem.AppDataDirectory;

            // Lista os arquivos e diretórios
            var diretorios = Directory.GetDirectories(baseDir);
            var arquivos = Directory.GetFiles(baseDir);

            // Cria uma string formatada com o conteúdo
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Diretório base: {baseDir}");
            sb.AppendLine("\nDiretórios:");
            foreach (var dir in diretorios)
            {
                sb.AppendLine($"- {Path.GetFileName(dir)}");
            }

            sb.AppendLine("\nArquivos:");
            foreach (var arquivo in arquivos)
            {
                sb.AppendLine($"- {Path.GetFileName(arquivo)}");
            }

            // Verifica arquivos .db3 especificamente
            sb.AppendLine("\nArquivos de banco de dados (*.db3):");
            var dbFiles = Directory.GetFiles(baseDir, "*.db3", SearchOption.AllDirectories);
            foreach (var db in dbFiles)
            {
                sb.AppendLine($"- {db}");
            }

            // Exibe o resultado
            await DisplayAlert("Arquivos e Diretórios", sb.ToString(), "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao listar diretórios: {ex.Message}", "OK");
        }
    }

}
