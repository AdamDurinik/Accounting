using Accounting.Models;
using DevExpress.Xpf.Grid;
using System.Collections;
using UserControl = System.Windows.Controls.UserControl;

namespace Accounting.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }
        private void OnAddingNewRow(object sender, System.ComponentModel.AddingNewEventArgs e)
        {
            var view = sender as TableView;
            if(view == null)
            {
                return;
            }

            var grid = view.Grid as GridControl;
            var column = grid.DataContext as ColumnModel;

            if(column == null)
            {
                return;
            }

            var item = new ItemModel(column);
            e.NewObject = item;
        }
    }
}
