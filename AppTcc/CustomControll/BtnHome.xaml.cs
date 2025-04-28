namespace AppTcc.CustomControll;

public partial class BtnHome : ContentView
{
    // Torne o evento público!
    public event EventHandler CancelarClicked;
    public event EventHandler AvancarClicked;

    public BtnHome()
    {
        InitializeComponent();
    }

    public async void Btn_Cancelar(object sender, EventArgs e)
    {
        var confirmar = await Application.Current.MainPage.DisplayAlert("Cancelar", "Deseja realmente cancelar?", "Sim", "Não");
        if (confirmar)
        {
            CancelarClicked?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Btn_Avancar(object sender, EventArgs e)
    {
        AvancarClicked?.Invoke(this, EventArgs.Empty);
    }
}
