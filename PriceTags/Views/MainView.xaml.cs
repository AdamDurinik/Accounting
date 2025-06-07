using PriceTags.Models;
using UserControl = System.Windows.Controls.UserControl;
namespace PriceTags.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void OnAddingNewRow(object sender, System.ComponentModel.AddingNewEventArgs e)
        {
            // Initialize a new row object here
            e.NewObject = new PriceTagModel(); // Replace 'PriceTag' with the appropriate type for your data
        }
    }
}
