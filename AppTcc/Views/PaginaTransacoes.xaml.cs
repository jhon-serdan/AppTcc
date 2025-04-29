using AppTcc.Helper;
using System.Threading.Tasks;
using System.Collections.ObjectModel;


namespace AppTcc.Views;

public partial class Transacoes : ContentPage
{
    ObservableCollection<Transacao> lista = new ObservableCollection<Transacao>();


	public Transacoes()
	{
		InitializeComponent();

        lst_Transacoes.ItemsSource = lista;

    }


    #region Limpar toda tabela de Transações
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
    #endregion

    protected async override void OnAppearing()
    {
        try
        {
            lista.Clear();

            List<Transacao> tmp = await App.DB.ListarTransacaoAsync();

            tmp.ForEach(i => lista.Add(i));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "oK");
        }

        
    }

}