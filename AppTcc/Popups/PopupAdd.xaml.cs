using CommunityToolkit.Maui.Views;

namespace AppTcc.Popups
{
    public partial class PopupAdd : Popup
    {
        public PopupAdd() // <- Construtor com o mesmo nome da classe!
        {
            InitializeComponent();
        }

        private void Receita_Clicked(object sender, EventArgs e)
        {
            Close(); // Aqui você pode chamar a página de cadastro de receita
        }

        private void Despesa_Clicked(object sender, EventArgs e)
        {
            Close(); // Aqui você pode chamar a página de cadastro de despesa
        }

        private void Transferencia_Clicked(object sender, EventArgs e)
        {
            Close();
        }

        private void Poupanca_Clicked(object sender, EventArgs e)
        {
            Close();
        }
    }
}
