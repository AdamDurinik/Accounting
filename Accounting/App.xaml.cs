using System.Windows;

namespace Accounting
{
    public partial class App : System.Windows.Application
    {
        private void OnStartUp(object sender, StartupEventArgs e)
        {
            var viewModel = new ViewModels.MainViewModel();
            var mainWindow = new MainWindow(viewModel);
            mainWindow.Title = "Účtovníctvo";
            viewModel.Window = mainWindow;
            mainWindow.Show();
        }
    }

}
