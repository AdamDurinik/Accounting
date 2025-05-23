using DevExpress.Xpf.Grid;
using System.Windows;

namespace Accounting
{
    public partial class App : System.Windows.Application
    {
        static App()
        {
            GridControl.AllowInfiniteGridSize = true;
        }

        private void OnStartUp(object sender, StartupEventArgs e)
        {
            var viewModel = new ViewModels.MainViewModel();
            var mainWindow = new MainWindow(viewModel);
            mainWindow.Title = "Účtovníctvo";
            viewModel.Window = mainWindow;
            mainWindow.Show();

            viewModel.LoadData();
        }
    }

}
