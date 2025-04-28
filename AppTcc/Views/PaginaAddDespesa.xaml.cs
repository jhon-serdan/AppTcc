using AppTcc.Helper;

namespace AppTcc.Views;

public partial class PaginaAddDespesa : ContentPage
{

    private SQLiteDatabaseHelper _conn;
    private List<Categoria> _categorias;

	public PaginaAddDespesa()
	{
		InitializeComponent();

        _conn = MauiProgram.Services.GetService<SQLiteDatabaseHelper>();
        BtnHomeDespesa.CancelarClicked += BtnHome_CancelarClicked;
        BtnHomeDespesa.AvancarClicked += BtnHome_AvancarClicked;
    }

    private async void BtnHome_CancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("PaginaInicial");
    }

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

    private async void BtnHome_AvancarClicked (object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(EntryValorDespesa.Text))
        {
            await DisplayAlert("Atenção", "Informe o valor", "OK");
            return;
        }

        if (Convert.ToDouble(EntryValorDespesa.Text) < 0)
        {
            await DisplayAlert("Atenção", "Informe valor maior que 0", "OK");
            return;
        }

        if (PckCategoriaDespesa.SelectedIndex <= 0)
        {
            await DisplayAlert("Atenção", "Selecione uma categoria", "OK");
            return;
        }

        bool eParcelado = RbParcelado.IsChecked;
        int? numeroParcelas = null;

        if (eParcelado)
        {
            if (string.IsNullOrEmpty(EntryParcelas.Text))
            {
                await DisplayAlert("Atenção", "Informe o número de Parcelas", "OK");
                return;
            }


            if (!int.TryParse(EntryParcelas.Text, out int parcelas) || parcelas <= 1)
            {
                await DisplayAlert("Atenção", "Número de Parcelas deve ser maior que 1", "OK");
                return;
            }

            numeroParcelas = parcelas;
        }

        var transacao = new Transacao();
        {
            Valor = EntryValorDespesa
        }

    }
}