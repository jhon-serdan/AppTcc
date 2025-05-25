using AppTcc.Helper;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AppTcc.Views;

[QueryProperty(nameof(TransacaoSelecionada), "TransacaoSelecionada")]
public partial class PaginaEditarItem : ContentPage, INotifyPropertyChanged
{
    private Transacao _transacaoSelecionada;
    public Transacao TransacaoSelecionada
    {
        get => _transacaoSelecionada;
        set
        {
            _transacaoSelecionada = value;
            OnPropertyChanged();
            CarregarCategorias();
            VerificarSeEParcelado();
        }
    }

    private List<Categoria> _categorias;
    public List<Categoria> Categorias
    {
        get => _categorias;
        set
        {
            _categorias = value;
            OnPropertyChanged();
        }
    }

    public List<string> CategoriasNomes => Categorias?.Select(c => c.Nome).ToList() ?? new List<string>();

    private string _categoriaSelecionada;
    public string CategoriaSelecionada
    {
        get => _categoriaSelecionada;
        set
        {
            _categoriaSelecionada = value;
            OnPropertyChanged();
        }
    }

    public PaginaEditarItem()
    {
        InitializeComponent();
        BindingContext = this;


        BtnHomeEditarItem.CancelarClicked += BtnCancelar_Clicked;
        BtnHomeEditarItem.AvancarClicked += BtnSalvar_Clicked;
    }

    #region Propriedades para controle das transações parceladas
    // Propriedades para controle de parcelas
    private bool _eTransacaoParcelada;
    public bool ETransacaoParcelada
    {
        get => _eTransacaoParcelada;
        set
        {
            _eTransacaoParcelada = value;
            OnPropertyChanged();
        }
    }

    private string _informacaoParcela;
    public string InformacaoParcela
    {
        get => _informacaoParcela;
        set
        {
            _informacaoParcela = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Método para verificar se a transação é parcelada
    private void VerificarSeEParcelado()
    {
        if (TransacaoSelecionada != null && TransacaoSelecionada.EParcelado)
        {
            ETransacaoParcelada = true;
            InformacaoParcela = $"Parcela {TransacaoSelecionada.ParcelaAtual} de {TransacaoSelecionada.NumeroParcelas}";
        }
        else
        {
            ETransacaoParcelada = false;
            InformacaoParcela = "";
        }
    }

    #endregion

    #region Método para carregar categorias do picker

    private async void CarregarCategorias()
    {
        try
        {
            if (TransacaoSelecionada != null)
            {
                TipoCategoria tipoCategoria = TransacaoSelecionada.Tipo == TipoTransacao.Receita
                    ? TipoCategoria.Receita
                    : TipoCategoria.Despesa;

                Categorias = await App.DB.ListaCategoria(tipoCategoria);
                OnPropertyChanged(nameof(CategoriasNomes));

                var categoriaAtual = Categorias.FirstOrDefault(c => c.Id == TransacaoSelecionada.CategoriaId);
                if (categoriaAtual != null)
                {
                    CategoriaSelecionada = categoriaAtual.Nome;
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar categorias: {ex.Message}", "OK");
        }
    }

    #endregion

    #region Métodos de Atualizar
    #region Método Salvar Transação
    private async void BtnSalvar_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (TransacaoSelecionada != null)
            {
                var categoriaEscolhida = Categorias.FirstOrDefault(c => c.Nome == CategoriaSelecionada);
                if (categoriaEscolhida != null)
                {
                    TransacaoSelecionada.CategoriaId = categoriaEscolhida.Id;
                }

                // Verificar se é uma transação parcelada
                if (TransacaoSelecionada.EParcelado && TransacaoSelecionada.NumeroParcelas > 1)
                {
                    bool confirmarAtualizacao = await DisplayAlert(
                        "Transação Parcelada",
                        "Esta é uma transação parcelada. Deseja atualizar todas as parcelas ou apenas esta?",
                        "Todas as parcelas",
                        "Apenas esta");

                    if (confirmarAtualizacao)
                    {
                        await AtualizarTodasAsParcelas();
                    }
                    else
                    {
                        await App.DB.SalvarTransacoesAsync(TransacaoSelecionada);
                    }
                }
                else
                {
                    await App.DB.SalvarTransacoesAsync(TransacaoSelecionada);
                }

                await DisplayAlert("Sucesso", "Transação atualizada com sucesso!", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao salvar: {ex.Message}", "OK");
        }
    }

    #endregion

    #region Método Atualizar Todas as Parcelas da Transação
    private async Task AtualizarTodasAsParcelas()
    {
        try
        {
            // Buscar todas as transações relacionadas
            var todasTransacoes = await App.DB.ListarTransacaoAsync();
            List<Transacao> parcelasRelacionadas;

            // Se é a primeira parcela (TransacaoOrigemId é null)
            if (TransacaoSelecionada.TransacaoOrigemId == null)
            {
                // Buscar todas as parcelas que têm esta transação como origem
                parcelasRelacionadas = todasTransacoes
                    .Where(t => t.TransacaoOrigemId == TransacaoSelecionada.Id || t.Id == TransacaoSelecionada.Id)
                    .OrderBy(t => t.ParcelaAtual)
                    .ToList();
            }
            else
            {
                // Buscar todas as parcelas que têm a mesma origem
                parcelasRelacionadas = todasTransacoes
                    .Where(t => t.TransacaoOrigemId == TransacaoSelecionada.TransacaoOrigemId ||
                               t.Id == TransacaoSelecionada.TransacaoOrigemId)
                    .OrderBy(t => t.ParcelaAtual)
                    .ToList();
            }


            int numeroParcelas = TransacaoSelecionada.NumeroParcelas ?? parcelasRelacionadas.Count;
            decimal valorTotal = TransacaoSelecionada.Valor;
            decimal novoValorParcela = Math.Round(valorTotal / numeroParcelas, 2);

            // Atualizar todas as parcelas
            foreach (var parcela in parcelasRelacionadas)
            {
                parcela.Valor = novoValorParcela;
                parcela.CategoriaId = TransacaoSelecionada.CategoriaId;
                parcela.Descricao = TransacaoSelecionada.Descricao;
                parcela.Conta = TransacaoSelecionada.Conta;

                await App.DB.SalvarTransacoesAsync(parcela);
            }

            await DisplayAlert("Sucesso",
                $"Todas as {parcelasRelacionadas.Count} parcelas foram atualizadas!\n" +
                $"Valor total: {valorTotal:C}\n" +
                $"Valor por parcela: {novoValorParcela:C}",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao atualizar parcelas: {ex.Message}", "OK");
        }
    }

    #endregion

    #endregion

    #region Métodos de excluir transação

    #region Método excluir transação
    private async void BtnExcluir_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (TransacaoSelecionada != null)
            {
                bool confirmarExclusao = await DisplayAlert(
                    "Confirmação",
                    "Tem certeza que deseja excluir esta transação?",
                    "Sim",
                    "Não");

                if (!confirmarExclusao) return;

                // Verificar se é uma transação parcelada
                if (TransacaoSelecionada.EParcelado && TransacaoSelecionada.NumeroParcelas > 1)
                {
                    bool excluirTodas = await DisplayAlert(
                        "Transação Parcelada",
                        "Esta é uma transação parcelada. Deseja excluir todas as parcelas ou apenas esta?",
                        "Todas as parcelas",
                        "Apenas esta");

                    if (excluirTodas)
                    {
                        await ExcluirTodasAsParcelas();
                    }
                    else
                    {
                        await App.DB.DeletarTransacao(TransacaoSelecionada);
                        await DisplayAlert("Sucesso", "Parcela excluída com sucesso!", "OK");
                    }
                }
                else
                {
                    await App.DB.DeletarTransacao(TransacaoSelecionada);
                    await DisplayAlert("Sucesso", "Transação excluída com sucesso!", "OK");
                }

                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao excluir: {ex.Message}", "OK");
        }
    }

    #endregion

    #region Método Excluir Todas as Parcelas

    private async Task ExcluirTodasAsParcelas()
    {
        try
        {
            // Buscar todas as transações relacionadas
            var todasTransacoes = await App.DB.ListarTransacaoAsync();

            List<Transacao> parcelasRelacionadas;

            // Se é a primeira parcela (TransacaoOrigemId é null)
            if (TransacaoSelecionada.TransacaoOrigemId == null)
            {
                // Buscar todas as parcelas que têm esta transação como origem
                parcelasRelacionadas = todasTransacoes
                    .Where(t => t.TransacaoOrigemId == TransacaoSelecionada.Id || t.Id == TransacaoSelecionada.Id)
                    .ToList();
            }
            else
            {
                // Buscar todas as parcelas que têm a mesma origem
                parcelasRelacionadas = todasTransacoes
                    .Where(t => t.TransacaoOrigemId == TransacaoSelecionada.TransacaoOrigemId ||
                               t.Id == TransacaoSelecionada.TransacaoOrigemId)
                    .ToList();
            }

            // Confirmar exclusão de múltiplas parcelas
            bool confirmarExclusaoTodas = await DisplayAlert(
                "Confirmação Final",
                $"Confirma a exclusão de todas as {parcelasRelacionadas.Count} parcelas desta transação?",
                "Sim, excluir todas",
                "Cancelar");

            if (!confirmarExclusaoTodas) return;

            // Excluir todas as parcelas
            foreach (var parcela in parcelasRelacionadas)
            {
                await App.DB.DeletarTransacao(parcela);
            }

            await DisplayAlert("Sucesso",
                $"Todas as {parcelasRelacionadas.Count} parcelas foram excluídas!",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao excluir parcelas: {ex.Message}", "OK");
        }
    }

    #endregion

    #endregion

    #region Método para cancelar a edição e voltar a página anterior
    private async void BtnCancelar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    #endregion

    #region Métodos para atualiar as binding

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}