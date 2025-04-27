using AppTcc.Popups;
using AppTcc.Views;
using CommunityToolkit.Maui.Views;

namespace AppTcc
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(PaginaAddDespesa), typeof(PaginaAddDespesa));
            Routing.RegisterRoute(nameof(PaginaAddPoupança), typeof(PaginaAddPoupança));
            Routing.RegisterRoute(nameof(PaginaAddReceita), typeof(PaginaAddReceita));
            Routing.RegisterRoute(nameof(PaginaAddTransferencia), typeof(PaginaAddTransferencia));
            Routing.RegisterRoute(nameof(PaginaInicial), typeof(PaginaInicial));

        }


    }
}
