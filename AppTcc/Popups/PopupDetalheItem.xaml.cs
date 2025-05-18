using AppTcc.Views;
using CommunityToolkit.Maui.Views;
using System.Threading.Tasks;

namespace AppTcc.Popups
{
    public partial class PopupDetalheItem : Popup
    {
        public PopupDetalheItem()
        {
            InitializeComponent();
        }


        private void Btn_Popup_Detalhe_Excluir(object sender, EventArgs e)
        {

        }

        private async void Btn_Popup_Detalhe_Editar(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(PaginaEditarItem));
            Close();
        }
    }
}
