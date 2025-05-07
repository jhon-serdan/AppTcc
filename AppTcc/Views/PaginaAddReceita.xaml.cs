namespace AppTcc.Views;

using AppTcc.Helper;

public partial class PaginaAddReceita : ContentPage
{
    private List<Categoria> _categorias;

    
    public PaginaAddReceita()
    {
        InitializeComponent();

        BtnHomeReceita.CancelarClicked += BtnHome_CancelarClicked;
        BtnHomeReceita.AvancarClicked += BtnHome_AvancarClicked;

        CarregarCategorias();
    }

    #region Logica Botao Cancelar
    private async void BtnHome_CancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("PaginaInicial");
    }

    #endregion

    #region Carrega as categorias de despesa
    private async void CarregarCategorias()
    {
        try
        {

            _categorias = await App.DB.ListaCategoria(TipoCategoria.Receita);

            PckCategoriaReceita.Items.Clear();
            PckCategoriaReceita.Items.Add("-- Selecione --");

            foreach (var categoria in _categorias)
            {
                PckCategoriaReceita.Items.Add(categoria.Nome);
            }

            PckCategoriaReceita.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar categorias {ex.Message}", "OK");
        }
    }

    #endregion

    #region Validação dos campos
    private bool ValidarFormularioReceita()
    {
        #region Validação Valor

        if (string.IsNullOrEmpty(EntryValorReceita.Text))
        {
            DisplayAlert("Erro", "Insira um valor válido", "OK");
            return false;
        }

        if (!decimal.TryParse(EntryValorReceita.Text, out decimal valor) || valor <= 0)
        {
            DisplayAlert("Erro", "Informe um valor válido", "OK");
            return false;
        }
        #endregion

        #region Validação Categoria

        if (PckCategoriaReceita.SelectedIndex <= 0 || PckCategoriaReceita.SelectedIndex == 0)
        {
            DisplayAlert("Erro", "Informe uma categoria", "OK");
            return false;
        }

        return true;

        #endregion
    }
    #endregion

    #region Logica botão avançar
    private async void BtnHome_AvancarClicked(object sender, EventArgs e)
    {
        if (!ValidarFormularioReceita())
            return;

        try
        {

            int categoriaIndex = PckCategoriaReceita.SelectedIndex;
            var categoriaSelecionada = _categorias[categoriaIndex - 1];

            var transacao = new Transacao
            {
                Valor = Convert.ToDecimal(EntryValorReceita.Text),
                Data = DtpckReceita.Date,
                CategoriaId = categoriaSelecionada.Id,
                Tipo = TipoTransacao.Receita,
                Descricao = DescricaoReceita.Text,
                Conta = "Carteira"
            };

                await App.DB.SalvarTransacoesAsync(transacao);

            await DisplayAlert("Sucesso", "Transação salva com sucesso!", "OK");
            await Shell.Current.GoToAsync("..");

        }
        catch (Exception ex)
        {
            await DisplayAlert("Atenção", $"Ocorreu um erro em: {ex.Message}", "OK");
        }
    }
#endregion
}