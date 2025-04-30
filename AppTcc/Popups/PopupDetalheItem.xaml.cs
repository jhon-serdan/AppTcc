using AppTcc.Views;
using CommunityToolkit.Maui.Views;
using System.Threading.Tasks;

namespace AppTcc.Popups
{
    public partial class PopupDetalheItem : Popup
    {
        private void Btn_Popup_Detalhe_Excluir(object sender, EventArgs e)
        {

        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(PaginaEditarItem));
        }
    }
}
