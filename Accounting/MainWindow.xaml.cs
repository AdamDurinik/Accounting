using Accounting.ViewModels;
using System.Windows;

namespace Accounting
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            MainView.DataContext = viewModel;
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var viewModel = (MainViewModel)MainView.DataContext;
            viewModel.Save(true);
        }
    }
}