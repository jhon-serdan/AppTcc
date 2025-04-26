namespace AppTcc.Views;

public partial class PaginaAddReceita : ContentPage
{
    public PaginaAddReceita()
    {
        InitializeComponent();

        BtnHomeReceita.CancelarClicked += BtnHome_CancelarClicked;
    }

    private async void BtnHome_CancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("PaginaInicial");
    }
}