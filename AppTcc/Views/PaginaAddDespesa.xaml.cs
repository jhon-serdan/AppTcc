namespace AppTcc.Views;

using AppTcc.Helper;
using CommunityToolkit.Maui.Views;

public partial class PaginaAddDespesa : ContentPage
{

    private List<Categoria> _categorias;

	public PaginaAddDespesa()
	{
		InitializeComponent();

        BtnHomeDespesa.CancelarClicked += BtnHome_CancelarClicked;
        BtnHomeDespesa.AvancarClicked += BtnHome_AvancarClicked;

        DtpckDespesa.Date = DateTime.Now;

        CarregarCategorias();

    }

    #region Carrega as categorias de despesa
    private async void CarregarCategorias()
    {
        try
        {
            
            _categorias = await App.DB.ListaCategoria(TipoCategoria.Despesa);

            PckCategoriaDespesa.Items.Clear();
            PckCategoriaDespesa.Items.Add("-- Selecione --");

            foreach (var categoria in _categorias)
            {
                PckCategoriaDespesa.Items.Add(categoria.Nome);
            }

            PckCategoriaDespesa.SelectedIndex = 0;
        } catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar categorias {ex.Message}", "OK");
        }
    }

    #endregion

    #region Logica botão cancelar
    private async void BtnHome_CancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("PaginaInicial");
    }

    #endregion

    #region Abilitar campo de parcelamento
    private void FormaPagamento_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender == RbParcelado && e.Value)
        {
            //se o botão de parcelado for selecionado, mostra o campo de parcelas
            CampoParcelas.IsVisible = true;
        }
        else if (sender == RbVista && e.Value)
        {
            //se o botão a vista for seleciona, esconde o campo de parcelas
            CampoParcelas.IsVisible = false;

            //apaga a quantidade de parcelas ao esconder o campo de parcelas
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

    #region Logica botão avançar
    private async void BtnHome_AvancarClicked (object sender, EventArgs e)
    {
        if (!ValidarFormularioDespesa())
            return;

        try
        {

            int categoriaIndex = PckCategoriaDespesa.SelectedIndex;
            var categoriaSelecionada = _categorias[categoriaIndex - 1];

            var transacao = new Transacao
            {
                Valor = Convert.ToDecimal(EntryValorDespesa.Text),
                Data = DtpckDespesa.Date,
                CategoriaId = categoriaSelecionada.Id,
                Tipo = TipoTransacao.Despesa,
                Descricao = DescricaoDespesa.Text,
                EParcelado = RbParcelado.IsChecked,
                Conta = "Carteira"
            };

            if (RbParcelado.IsChecked && !string.IsNullOrEmpty(EntryParcelas.Text))
            {
                transacao.NumeroParcelas = Convert.ToInt32(EntryParcelas.Text);
                await App.DB.SalvarTransacaoParcelada(transacao);
            }
            else
            {
                await App.DB.SalvarTransacoesAsync(transacao);
            }

            await DisplayAlert("Sucesso", "Transação salva com sucesso!", "OK");
            await Shell.Current.GoToAsync("PaginaInicial");

        }catch (Exception ex)
        {
            await DisplayAlert("Atenção", $"Ocorreu um erro em: {ex.Message}", "OK");
        }
    }
    #endregion
}