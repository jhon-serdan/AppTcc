using AppTcc.Helper;

namespace AppTcc.Views;

public partial class PaginaAddDespesa : ContentPage
{

    private SQLiteDatabaseHelper _conn;
    private List<Categoria> _categorias;

	public PaginaAddDespesa()
	{
		InitializeComponent();

        _conn = MauiProgram.CreateMauiApp().Services.GetServices<SQLiteDatabaseHelper>();

        BtnHomeDespesa.CancelarClicked += BtnHome_CancelarClicked;
        BtnHomeDespesa.AvancarClicked += BtnHome_AvancarClicked;

        CarregarCategorias();
    }

    private async void CarregarCategorias()
    {
        try
        {
            _categorias = await _conn.ListaCategoria(TipoCategoria.Despesa);

            PckCategoriaDespesa.Items.Clear();
            PckCategoriaDespesa.Items.Add("-- Selecione --");

            foreach (var categoria in _categorias)
            {
                PckCategoriaDespesa.Items.Add(categoria.Nome);
            }

            PckCategoriaDespesa.SelectedIndex = 0;
        } 
        catch (Exception ex)
        {
            await DisplayAlert("Atenção", $"Erro ao carregar categorias: {ex.Message}", "OK");
        }
    }

    private async void BtnHome_CancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("PaginaInicial");
    }

    #region Abilitar campo de parcelamento
    private void FormaPagamento_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender == RbParcelado && e.Value)
        {
            CampoParcelas.IsVisible = true;
        }
        else if (sender == RbVista && e.Value)
        {
            CampoParcelas.IsVisible = false;

            EntryParcelas.Text = string.Empty;
        }
    }
    #endregion

    #region Validação dos campos
    private bool ValidarFormularioDespesa ()
    {
        #region Validação Valor

        if (string.IsNullOrEmpty(EntryValorDespesa.Text))
        {
            DisplayAlert("Erro", "Insira um valor válido", "OK");
            return false;
        }

        if (!decimal.TryParse(EntryValorDespesa.Text, out decimal valor) || valor <= 0)
        {
            DisplayAlert("Erro", "Informe um valor válido", "OK");
            return false;
        }
        #endregion

        #region Validação Categoria

        if (PckCategoriaDespesa.SelectedIndex <= 0 || PckCategoriaDespesa.SelectedIndex == 0)
        {
            DisplayAlert("Erro", "Informe uma categoria", "OK");
            return false;
        }

        #endregion

        #region Validação Parcelas

        if (RbParcelado.IsChecked)
        {
            if (string.IsNullOrEmpty(EntryParcelas.Text))
            {
                DisplayAlert("Erro", "Insira as parcelas", "OK");
                return false;
            } 

            if (!int.TryParse(EntryParcelas.Text, out int parcelas) || parcelas <= 1)
            {
                DisplayAlert("Erro", "Número de Parcelas inválido", "OK");
                return false;
            }
        }

        return true;

        #endregion
    }
    #endregion
    private async void BtnHome_AvancarClicked (object sender, EventArgs e)
    {
        if (!ValidarFormularioDespesa())
            return;

        try
        {
            var categoriaSeleciona = _categorias[categoriaSeleciona - 1];

            var transacao = new Transacao
            {
                Valor = Convert.ToDecimal(EntryValorDespesa.Text),
                Data = DtpckDespesa.Date,
                CategoriaId = categoriaSeleciona.Id,
                Tipo = TipoTransacao.Despesa,
                Descricao = DescricaoDespesa.text,
                EParcelado = RbParcelado.IsChecked,
                Conta = "Carteira"
            };

            if (RbParcelado.IsChecked || !string.IsNullOrEmpty(EntryParcelas.Text))
            {
                transacao.NumeroParcelas = Convert.ToInt32(EntryParcelas.Text);
                await SQLiteDatabaseHelper.SalvarTransacaoParcelada(transacao);
            }
            else
            {
                await SQLiteDatabaseHelper.SalvarTransacoesAsync.(transacao);
            }

            await DisplayAlert("Sucesso", "Transação salva com sucesso!", "OK");
            await Shell.Current.GoToAsync("//PaginaInicial");

        }catch (Exception ex)
        {
            await DisplayAlert("Atenção", $"Ocorreu um erro em: {ex.Message}", "OK");
        }
    }
}