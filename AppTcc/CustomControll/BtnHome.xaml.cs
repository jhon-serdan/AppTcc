namespace AppTcc.CustomControll;

public partial class BtnHome : ContentView
{
    // Torne o evento p�blico!
    public event EventHandler CancelarClicked;

    public BtnHome()
    {
        InitializeComponent();
    }

    private async void Btn_Cancelar(object sender, EventArgs e)
    {
        var confirmar = await Application.Current.MainPage.DisplayAlert("Cancelar", "Deseja realmente cancelar?", "Sim", "N�o");
        if (confirmar)
        {
            CancelarClicked?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Btn_Avancar(object sender, EventArgs e)
    {
        // l�gica futura
    }
}
