using AppTcc.Helper;
using System.Collections.ObjectModel;

namespace AppTcc.Views
{
    public partial class Transacoes : ContentPage
    {
        public ObservableCollection<Transacao> lista { get; set; }
        public Transacao ItemSelecionado { get; set; }

        public Transacoes()
        {
            InitializeComponent();
            lista = new ObservableCollection<Transacao>();
            BindingContext = this;

            // Carrega todas as transa��es inicialmente
            CarregarTransacoes();

            PckTransacoes.SelectedIndex = 0;
        }

        private async void CarregarTransacoes()
        {
            try
            {
                var transacoes = await App.DB.ListarTransacaoAsync();
                AtualizarLista(transacoes);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar transa��es: {ex.Message}", "OK");
            }
        }

        private async void CarregarTransacoesPorPeriodo(DateTime dataEscolhida)
        {
            try
            {
                int mes = dataEscolhida.Month;
                int ano = dataEscolhida.Year;

                var transacoes = await App.DB.ListarTransacaoMes(mes, ano);
                AtualizarLista(transacoes);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar transa��es do per�odo: {ex.Message}", "OK");
            }
        }

        private void AtualizarLista(List<Transacao> transacoes)
        {
            lista.Clear();

            if (transacoes != null && transacoes.Count > 0)
            {
                foreach (var transacao in transacoes)
                {
                    lista.Add(transacao);
                }

                // Mostra a lista e esconde a mensagem
                lst_Transacoes.IsVisible = true;
                LblSemTransacoes.IsVisible = false;
            }
            else
            {
                // Esconde a lista e mostra a mensagem
                lst_Transacoes.IsVisible = false;
                LblSemTransacoes.IsVisible = true;
            }
        }

        private void PckTransacoes_SelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;

            if (picker.SelectedIndex == 0) // Geral
            {
                GridPeriodo.IsVisible = false;
                CarregarTransacoes(); // Carrega todas as transa��es
            }
            else if (picker.SelectedIndex == 1) // Per�odo
            {
                GridPeriodo.IsVisible = true;
                // Carregar transa��es do m�s atual como padr�o
                CarregarTransacoesPorPeriodo(DateTime.Now);
            }
        }

        private void DtPckTransacao_DateSelected(object sender, DateChangedEventArgs e)
        {
            // Quando uma nova data � selecionada, carrega as transa��es desse per�odo
            CarregarTransacoesPorPeriodo(e.NewDate);
        }

        private async void lst_Transacoes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Transacao transacaoSelecionada)
            {
                // Aqui voc� pode implementar a l�gica para quando uma transa��o � selecionada
                // Por exemplo, navegar para uma p�gina de detalhes ou edi��o

                // Limpa a sele��o
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        private async void Btn_LimparTransacao(object sender, EventArgs e)
        {
            try
            {
                bool resposta = await DisplayAlert(
                    "Confirma��o",
                    "Tem certeza que deseja excluir todas as transa��es? Esta a��o n�o pode ser desfeita.",
                    "Sim",
                    "Cancelar");

                if (resposta)
                {
                    await App.DB.LimparTabelaTransacoes();

                    // Atualiza a lista (que ficar� vazia)
                    AtualizarLista(new List<Transacao>());

                    await DisplayAlert("Sucesso", "Todas as transa��es foram exclu�das.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao limpar transa��es: {ex.Message}", "OK");
            }
        }

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
}