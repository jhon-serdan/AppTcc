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
		bool answer = await DisplayAlert("Atenção", "Deseja zerar todas as transações. Essa ação não pode ser desfeita", "Sim", "Não");

		if (answer)
		{
			try
			{
				await App.DB.LimparTabelaTransacoes();

				await DisplayAlert ("Sucesso", "Todas as transações foram excluídas", "OK");
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