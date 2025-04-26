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
}