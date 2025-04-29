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
        CancelarClicked?.Invoke(this, EventArgs.Empty);
    }

    public void Btn_Avancar(object sender, EventArgs e)
    {
        AvancarClicked?.Invoke(this, EventArgs.Empty);
    }
}
