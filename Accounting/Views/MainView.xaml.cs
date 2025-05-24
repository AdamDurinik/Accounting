using Accounting.Models;
using DevExpress.Xpf.Grid;
using System.Collections.ObjectModel;
using System.Windows.Input;
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

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var grid = sender as GridControl;
            var view = grid?.View as TableView;
            if (view == null || grid == null || view.DataContext == null )
            {
                return;
            }

            int visibleIndex = grid.GetRowVisibleIndexByHandle(view.FocusedRowHandle);
            if (e.Key == Key.Enter && visibleIndex == (grid.VisibleRowCount - 1))
            {
                view.AddNewRow();
            } else if (e.Key == Key.Enter && visibleIndex != (grid.VisibleRowCount - 1))
            {
                grid.CurrentItem = ((ObservableCollection<ItemModel>)grid.ItemsSource).ElementAtOrDefault(visibleIndex + 1);
                view.ShowEditor();
            }
            
        }
    }
}
