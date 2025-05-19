using AppTcc.Helper;

namespace AppTcc.Views;

[QueryProperty(nameof(ItemId), "Id")]
public partial class PaginaEditarItem : ContentPage
{
	private int _itemId;
	public int ItemId
	{
		get => _itemId;
		set
		{
			_itemId = value;
			OnPropertyChanged();
			CarregarDetalheTransacao(value);
        }
    }

	private Transacao _transacao;
	public Transacao Transacao
	{
		get => _transacao;
		set
		{
			_transacao = value;
			OnPropertyChanged();
        }

    }


    public PaginaEditarItem()
	{
		InitializeComponent();
		_transacaoService = TransacaoService;
		BindingContext = this;
    }

	private async void CarregarDetalheTransacao(int itemId)
    {
		if(itemId != 0)
		{
			try
			{

				Transacao = await App.DB.GetTransacaoById(itemId);

            }
            catch (Exception ex)
			{
				await DisplayAlert("Erro", $"Ocorreu um erro: {ex.Message}", "OK");
            }
        }
    }