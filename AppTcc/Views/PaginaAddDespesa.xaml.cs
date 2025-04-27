namespace AppTcc.Views;

public partial class PaginaAddDespesa : ContentPage
{
	public PaginaAddDespesa()
	{
		InitializeComponent();

        BtnHomeDespesa.CancelarClicked += BtnHome_CancelarClicked;
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
}