using PriceTags.ViewModels;
using System.Windows.Input;

namespace PriceTags.Views
{
    public partial class SettingsView : System.Windows.Controls.UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void PreviewBorder_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is not SettingsViewModel vm) return;
            decimal delta = e.Delta > 0 ? 1m : -1m;
            vm.TagWidthMm = Math.Clamp(vm.TagWidthMm + delta, 20m, 190m);
            e.Handled = true;
        }
    }
}
