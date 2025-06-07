using System.Windows;

namespace PriceTags
{
    public partial class App : System.Windows.Application
    {
        public void OnStartup(object sender, StartupEventArgs e)
        {
            var viewModel = new ViewModels.MainViewModel();
            var mainWindow = new MainWindow(viewModel);
            mainWindow.Show();
        }
    }

}
