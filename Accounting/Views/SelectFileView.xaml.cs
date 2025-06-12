using System.Windows;
using System.Windows.Input;

namespace Accounting.Views
{
    public partial class SelectFileView : System.Windows.Controls.UserControl
    {
        public SelectFileView()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as System.Windows.Controls.ListBox;
            if (listBox?.SelectedItem != null)
            {
                var viewModel = DataContext as ViewModels.SelectFileViewModel;
                viewModel?.OkCommand.Command.Execute(null);
                var parentWindow = Window.GetWindow(this);
                parentWindow?.Close();
            }
        }
    }
}
