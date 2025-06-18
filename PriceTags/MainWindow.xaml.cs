using PriceTags.ViewModels;
using System.Windows;

namespace PriceTags
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
            viewModel.SaveItems();
        }
    }
}