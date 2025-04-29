using AppTcc.Helper;
using System.Threading.Tasks;

namespace AppTcc.Views;

public partial class Transacoes : ContentPage
{
	public Transacoes()
	{
		InitializeComponent();
	}

    private async Task LimparTodasTransacoes()
    {
		bool answer = await DisplayAlert("Aten��o", "Deseja zerar todas as transa��es. Essa a��o n�o pode ser desfeita", "Sim", "N�o");

		if (answer)
		{
			try
			{
				await App.DB.LimparTabelaTransacoes();

				await DisplayAlert ("Sucesso", "Todas as transa��es foram exclu�das", "OK");
            }
            catch (Exception ex)
			{
                await DisplayAlert("Erro", $"Ocorreu um erro: {ex.Message}", "OK");
            }

        }

    }

    private async void Btn_LimparTransacao(object sender, EventArgs e)
    {
		await LimparTodasTransacoes();
    }
}