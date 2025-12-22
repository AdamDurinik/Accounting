using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Grid;
using DevExpress.XtraGrid.Views.Base;
using PriceTags.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ((INotifyCollectionChanged)PriceTagGrid.ItemsSource).CollectionChanged += OnCollectionChanged;
        }

        private void OnSelectedCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var viewModel = DataContext as ViewModels.MainViewModel;
            viewModel.PageCountAndTagCount = viewModel.GetNewPageCountTagCount();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            var viewModel = DataContext as ViewModels.MainViewModel;
            viewModel.PriceTags.ForEach(t => t.Id = viewModel.PriceTags.IndexOf(t) + 1);
        }

        private void OnSelectionChanged(object? sender, object e)
        {
            var viewModel = DataContext as ViewModels.MainViewModel;
            viewModel.PageCountAndTagCount = viewModel.GetNewPageCountTagCount();
        }

        private void OnAddingNewRow(object sender, System.ComponentModel.AddingNewEventArgs e)
        {
            var model = new PriceTagModel();
            model.Id = ((ObservableCollection<PriceTagModel>)((TableView)sender).Grid.ItemsSource).Count + 1;
            
            e.NewObject = model;
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                var grid = sender as GridControl;
                var view = grid?.View as TableView;
                if (view == null || grid == null || view.DataContext == null)
                {
                    return;
                }

                int visibleIndex = grid.GetRowVisibleIndexByHandle(view.FocusedRowHandle);
                if (e.Key == Key.Enter && visibleIndex >= (grid.VisibleRowCount - 2))
                {
                    if(visibleIndex == grid.VisibleRowCount - 2)
                    {
                        view.FocusedRowHandle = GridControl.NewItemRowHandle;
                        view.ShowEditor();
                        e.Handled = true;
                        return;
                    }
                    view.AddNewRow();
                    view.ShowEditor();
                    e.Handled = true;
                    return;
                }
                else if (e.Key == Key.Enter && visibleIndex != (grid.VisibleRowCount - 1))
                {
                    grid.CurrentItem = ((ObservableCollection<PriceTagModel>)grid.ItemsSource).ElementAtOrDefault(visibleIndex + 1);
                    view.ShowEditor();
                    e.Handled = true;
                } else if(e.Key == Key.Right)
                {
                    view.MoveNextCell();
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private void OnCurrentColumnChanged(object sender, CurrentColumnChangedEventArgs e)
        {
            BaseView? view = sender as BaseView;
            if (view == null)
            {
                return;
            }

            view.ShowEditor();
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
