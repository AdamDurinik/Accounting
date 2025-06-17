using DevExpress.Xpf.Editors;
using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid.Views.Grid;
using PriceTags.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
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
            var model = new PriceTagModel();
            e.NewObject = model;
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var grid = sender as GridControl;
            var view = grid?.View as TableView;
            if (view == null || grid == null || view.DataContext == null)
            {
                return;
            }

            int visibleIndex = grid.GetRowVisibleIndexByHandle(view.FocusedRowHandle);
            if (e.Key == Key.Enter && visibleIndex == (grid.VisibleRowCount - 1))
            {
                view.AddNewRow();
            }
            else if (e.Key == Key.Enter && visibleIndex != (grid.VisibleRowCount - 1))
            {
                grid.CurrentItem = ((ObservableCollection<PriceTagModel>)grid.ItemsSource).ElementAtOrDefault(visibleIndex + 1);
                view.ShowEditor();
            }

        }

        void TableView_ShowingEditor(object sender, ShowingEditorEventArgs e)
        {
            if (e.Column.FieldName == "DepositAmount")
            {
                var row = e.Row as PriceTagModel;
                if (row != null && row.DepositAmount == 0)
                {
                    row.DepositAmount = 0.15;
                }
            }
            else if(e.Column.FieldName == "Name")
            {
                var row = e.Row as PriceTagModel;
                if(row == null)
                {
                    _currentlyEditing = null;
                    return;
                }

                _currentlyEditing = row.Name;
            }
        }

        void TableView_HidingEditor(object sender, EditorEventArgs e)
        {
            if(e.Column.FieldName == "Name")
            {
                var row = e.Row as PriceTagModel;
                var viewModel = DataContext as ViewModels.MainViewModel;
                viewModel?.SaveNameToFile(_currentlyEditing?.Trim(), row?.Name);
            }
        }

        private string? _currentlyEditing = null;
    }
}
