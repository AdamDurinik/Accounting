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
    }
}