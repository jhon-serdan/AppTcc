using AppTcc.Views;
using CommunityToolkit.Maui.Views;
using System.Threading.Tasks;

namespace AppTcc.Popups
{
    public partial class PopupAdd : Popup
    {
        public PopupAdd() // <- Construtor com o mesmo nome da classe!
        {
            InitializeComponent();
        }

        private async void Btn_AddReceita_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(PaginaAddReceita));
            Close();
        }

        private async void Btn_AddDespesa_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(PaginaAddDespesa));
            Close();
        }
    }
}
