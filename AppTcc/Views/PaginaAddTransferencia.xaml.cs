namespace AppTcc.Views;

public partial class PaginaAddTransferencia : ContentPage
{
    public PaginaAddTransferencia()
    {
        InitializeComponent();

        BtnHomeTransferencia.CancelarClicked += BtnHome_CancelarClicked;
    }

    private async void BtnHome_CancelarClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("PaginaInicial");
    }
}