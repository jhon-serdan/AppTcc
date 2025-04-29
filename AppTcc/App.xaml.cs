using AppTcc.Helper;

namespace AppTcc
{
    public partial class App : Application
    {

        static SQLiteDatabaseHelper _db;

        public static SQLiteDatabaseHelper DB
        {
            get
            {
                if (_db == null)
                {
                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "banco_financas.db3");

                    _db = new SQLiteDatabaseHelper(path);
                }

                return _db;
            }
        }

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}