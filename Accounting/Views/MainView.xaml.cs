using DevExpress.Xpf.Grid;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TextBox = System.Windows.Controls.TextBox;
using UserControl = System.Windows.Controls.UserControl;

namespace Accounting.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            MainGrid.Columns.Clear();

            var viewModel = DataContext as ViewModels.MainViewModel;
            if (viewModel != null)
            {
                RefreshColumns();
                viewModel.Columns.CollectionChanged += (_, __) => RefreshColumns();
            }

        }

        private void RefreshColumns()
        {
            var viewModel = DataContext as ViewModels.MainViewModel;

            MainGrid.Columns.Clear();

            MainGrid.Columns.Add(new GridColumn
            {
                FieldName = "TotalPrice",
                Header = "Celková cena",
                AllowEditing = DevExpress.Utils.DefaultBoolean.True
            });

            foreach (var def in viewModel.Columns)
            {
                var index = viewModel.Columns.IndexOf(def);
                var column = new GridColumn
                {
                    FieldName= $"{def.Name}",
                    UnboundType = DevExpress.Data.UnboundColumnType.Decimal,
                    UnboundExpression = $"[TotalPrice] * {def.Percentage/100}",
                    AllowEditing = DevExpress.Utils.DefaultBoolean.False,
                };

                // Bind Header with editable template
                column.HeaderTemplate = CreateHeaderTemplate(def, column);
                MainGrid.Columns.Add(column);
            }
        }

        private DataTemplate CreateHeaderTemplate(Models.ColumnDefinition def, GridColumn column)
        {
            var template = new DataTemplate();

            var stack = new FrameworkElementFactory(typeof(StackPanel));
            stack.SetValue(StackPanel.OrientationProperty, System.Windows.Controls.Orientation.Horizontal);

            var box = new FrameworkElementFactory(typeof(TextBox));
            box.SetValue(TextBox.WidthProperty, 50.0);
            box.SetValue(TextBox.TextProperty, column.Header); 

            box.AddHandler(TextBox.LostFocusEvent, new RoutedEventHandler((s, e) =>
            {
                if (s is TextBox tb)
                {
                    string text = tb.Text.Replace("%", "").Trim();
                    if (double.TryParse(text, out double percent))
                    {
                        percent = Math.Clamp(percent, 0, 100);
                        tb.Text = $"{percent}%";
                        column.Header = $"{percent}%";
                        column.UnboundExpression = $"[TotalPrice] * {percent / 100.0}";
                    }
                }
            }));

            stack.AppendChild(box);
            template.VisualTree = stack;
            return template;
        }


        private void OnNewRow(object sender, InitNewRowEventArgs e)
        {
            var viewModel = DataContext as ViewModels.MainViewModel;
            if (viewModel != null)
            {
                viewModel.Items.Add(new Models.ItemModel());
            }
        }
    }
}
