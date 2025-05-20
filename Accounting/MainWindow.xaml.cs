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
    }
}