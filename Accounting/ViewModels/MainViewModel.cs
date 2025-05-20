using Accounting.Models;
using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Accounting.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Columns = new ObservableCollection<Models.ColumnDefinition>();
            LoadItemsFromCsv();

            Items.Add(new ItemModel("1, 120, 50"));
        }

        public ICommand NewCommand => new DelegateCommand(New);
        public ICommand LoadCsvCommand => new DelegateCommand(OpenLoadWindow);
        public ICommand SaveCsvCommand => new DelegateCommand(Save);
        public ICommand ExportToExcelCommand => new DelegateCommand(ExportToExcel);
        public ICommand AddColumnCommand => new DelegateCommand(AddColumn);
      
        public ObservableCollection<Models.ColumnDefinition> Columns { get; set; }

        public ObservableCollection<ItemModel> Items { get; set; } = new ObservableCollection<ItemModel>();
        public MainWindow Window { get; internal set; }
        public string SelectedFile { get; internal set; }

        private void New()
        {
            Columns.Clear();
            Items.Clear();
        }

        private void OpenLoadWindow()
        {

        }
        private void Save()
        {
            throw new NotImplementedException();
        }
        private void ExportToExcel()
        {
            throw new NotImplementedException();
        }
        private void AddColumn()
        {
            var column = new Models.ColumnDefinition()
            {
                Name = $"C{Columns.Count()+1}",
                Percentage = 20
            };
            Columns.Add(column);
        }

        private void LoadItemsFromCsv(string path = null)
        {
            if (path == null)
            {
                try
                {
                    using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\FoxHint\Accounting"))
                    {
                        if (key == null) return;

                        var regPath = key.GetValue("CsvFilePath") as string;
                        if (string.IsNullOrWhiteSpace(regPath)) return;

                        path = regPath;
                    }
                }
                catch
                {
                    return;
                }
            }

            if(string.IsNullOrWhiteSpace(path) || !System.IO.File.Exists(path)) return;

            Window.Title = string.Format("Účtovníctvo - {0}", path);

            var lines = System.IO.File.ReadAllLines(path);
            if (lines.Length == 0) return;

            Columns.Clear();
            var headers = lines[0].Split(',');
            for (int i = 0; i < headers.Length; i++)
            {
                if (double.TryParse(headers[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double percent))
                {
                    var column = new Models.ColumnDefinition
                    {
                        Name = $"C{i+1}",
                        Percentage = (int)(percent * 100),
                    };
                    Columns.Add(column);
                }
            }

            Items.Clear();
            for (int i = 1; i < lines.Length; i++)
            {
                Items.Add(new ItemModel(lines[i]));
            }
        }

    }
}
