using AppTcc.Popups; // <- esse é o que importa aqui
using CommunityToolkit.Maui.Views;

namespace AppTcc.Views;

public partial class PaginaInicial : ContentPage
{
    public PaginaInicial()
    {
        InitializeComponent();
    }

    private void FloatingActionButton_Clicked(object sender, EventArgs e)
    {
        var popup = new PopupAdd(); // ou AdicionarPopup, se esse for o nome da classe
        this.ShowPopup(popup);
    }
}
