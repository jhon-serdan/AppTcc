using AppTcc.Popups; // <- esse é o que importa aqui
using CommunityToolkit.Maui.Views;

namespace AppTcc.Views;

public partial class PaginaInicial : ContentPage
{
    public PaginaInicial()
    {
        InitializeComponent();
    }

    private void FloatingActionButton_Clicked(object sender, EventArgs e)
    {
        var popup = new PopupAdd(); // ou AdicionarPopup, se esse for o nome da classe
        this.ShowPopup(popup);
    }

    private async void ExportarBanco_Clicked(object sender, EventArgs e)
    {
        await ExportarBancoAsync();
    }

    public async Task ExportarBancoAsync()
    {
        try
        {
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "banco_financas.db3");

            string exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "banco_financas.db3");

            if(DeviceInfo.Platform == DevicePlatform.Android)
            {
                exportPath = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Download", "banco_financas.db3");
            }

            File.Copy(dbPath, exportPath, true);

            await App.Current.MainPage.DisplayAlert("Sucesso", $"banco exportado para: {exportPath}", "OK");
        } catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Erro", $"Falha ao exportar: {ex.Message}", "OK");
        }
    }

}
