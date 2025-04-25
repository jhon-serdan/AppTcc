using AppTcc.Popups;
using CommunityToolkit.Maui.Views;

namespace AppTcc
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        // Método chamado quando o botão é clicado
        private async void OnOpenPopupClicked(object sender, EventArgs e)
        {
            var popup = new PopupAdd(); // Instancia o Popup
            await this.ShowPopupAsync(popup);  // Exibe o Popup
        }
    }
}
