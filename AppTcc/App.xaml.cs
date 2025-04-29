using AppTcc.Helper;

namespace AppTcc
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            InitializeComponent();

            MainPage = new AppShell();
        }

        private async void InitializeDatabase()
        {
            var dbHelper = IPlatformApplication.Current.Services.GetService<SQLiteDatabaseHelper>();
            if (dbHelper != null)
            {
                await dbHelper.InitializeDatabase();
            }
        }

    }
}