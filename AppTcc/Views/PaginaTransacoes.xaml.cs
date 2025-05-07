using AppTcc.Helper;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using AppTcc.Popups;


namespace AppTcc.Views;

public partial class Transacoes : ContentPage
{
    ObservableCollection<Transacao> lista = new ObservableCollection<Transacao>();


	public Transacoes()
	{
		InitializeComponent();

        lst_Transacoes.ItemsSource = lista;

    }


    #region Limpar toda tabela de Transa��es
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
    #endregion

    #region Limpa a lista de transa��o para evitar valor repetido

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

    #endregion

    private async void lst_Transacoes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not null)
        {
            await App.Current.MainPage.ShowPopupAsync(new PopupDetalheItem());
            lst_Transacoes.SelectedItem = null;
        }
    }
}