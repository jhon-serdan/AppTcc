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

    #region Propriedades para controle das transa��es parceladas
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

    #region M�todo para verificar se a transa��o � parcelada
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

    #region M�todo para carregar categorias do picker

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

    #region M�todos de Atualizar
    #region M�todo Salvar Transa��o
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

                // Verificar se � uma transa��o parcelada
                if (TransacaoSelecionada.EParcelado && TransacaoSelecionada.NumeroParcelas > 1)
                {
                    bool confirmarAtualizacao = await DisplayAlert(
                        "Transa��o Parcelada",
                        "Esta � uma transa��o parcelada. Deseja atualizar todas as parcelas ou apenas esta?",
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

                await DisplayAlert("Sucesso", "Transa��o atualizada com sucesso!", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao salvar: {ex.Message}", "OK");
        }
    }

    #endregion

    #region M�todo Atualizar Todas as Parcelas da Transa��o
    private async Task AtualizarTodasAsParcelas()
    {
        try
        {
            // Buscar todas as transa��es relacionadas
            var todasTransacoes = await App.DB.ListarTransacaoAsync();
            List<Transacao> parcelasRelacionadas;

            // Se � a primeira parcela (TransacaoOrigemId � null)
            if (TransacaoSelecionada.TransacaoOrigemId == null)
            {
                // Buscar todas as parcelas que t�m esta transa��o como origem
                parcelasRelacionadas = todasTransacoes
                    .Where(t => t.TransacaoOrigemId == TransacaoSelecionada.Id || t.Id == TransacaoSelecionada.Id)
                    .OrderBy(t => t.ParcelaAtual)
                    .ToList();
            }
            else
            {
                // Buscar todas as parcelas que t�m a mesma origem
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

    #region M�todos de excluir transa��o

    #region M�todo excluir transa��o
    private async void BtnExcluir_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (TransacaoSelecionada != null)
            {
                bool confirmarExclusao = await DisplayAlert(
                    "Confirma��o",
                    "Tem certeza que deseja excluir esta transa��o?",
                    "Sim",
                    "N�o");

                if (!confirmarExclusao) return;

                // Verificar se � uma transa��o parcelada
                if (TransacaoSelecionada.EParcelado && TransacaoSelecionada.NumeroParcelas > 1)
                {
                    bool excluirTodas = await DisplayAlert(
                        "Transa��o Parcelada",
                        "Esta � uma transa��o parcelada. Deseja excluir todas as parcelas ou apenas esta?",
                        "Todas as parcelas",
                        "Apenas esta");

                    if (excluirTodas)
                    {
                        await ExcluirTodasAsParcelas();
                    }
                    else
                    {
                        await App.DB.DeletarTransacao(TransacaoSelecionada);
                        await DisplayAlert("Sucesso", "Parcela exclu�da com sucesso!", "OK");
                    }
                }
                else
                {
                    await App.DB.DeletarTransacao(TransacaoSelecionada);
                    await DisplayAlert("Sucesso", "Transa��o exclu�da com sucesso!", "OK");
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

    #region M�todo Excluir Todas as Parcelas

    private async Task ExcluirTodasAsParcelas()
    {
        try
        {
            // Buscar todas as transa��es relacionadas
            var todasTransacoes = await App.DB.ListarTransacaoAsync();

            List<Transacao> parcelasRelacionadas;

            // Se � a primeira parcela (TransacaoOrigemId � null)
            if (TransacaoSelecionada.TransacaoOrigemId == null)
            {
                // Buscar todas as parcelas que t�m esta transa��o como origem
                parcelasRelacionadas = todasTransacoes
                    .Where(t => t.TransacaoOrigemId == TransacaoSelecionada.Id || t.Id == TransacaoSelecionada.Id)
                    .ToList();
            }
            else
            {
                // Buscar todas as parcelas que t�m a mesma origem
                parcelasRelacionadas = todasTransacoes
                    .Where(t => t.TransacaoOrigemId == TransacaoSelecionada.TransacaoOrigemId ||
                               t.Id == TransacaoSelecionada.TransacaoOrigemId)
                    .ToList();
            }

            // Confirmar exclus�o de m�ltiplas parcelas
            bool confirmarExclusaoTodas = await DisplayAlert(
                "Confirma��o Final",
                $"Confirma a exclus�o de todas as {parcelasRelacionadas.Count} parcelas desta transa��o?",
                "Sim, excluir todas",
                "Cancelar");

            if (!confirmarExclusaoTodas) return;

            // Excluir todas as parcelas
            foreach (var parcela in parcelasRelacionadas)
            {
                await App.DB.DeletarTransacao(parcela);
            }

            await DisplayAlert("Sucesso",
                $"Todas as {parcelasRelacionadas.Count} parcelas foram exclu�das!",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao excluir parcelas: {ex.Message}", "OK");
        }
    }

    #endregion

    #endregion

    #region M�todo para cancelar a edi��o e voltar a p�gina anterior
    private async void BtnCancelar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    #endregion

    #region M�todos para atualiar as binding

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}