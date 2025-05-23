using Accounting.Entity;
using Accounting.Models;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Xpf;
using DevExpress.Xpf.Grid;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Accounting.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
        }

        public ICommand NewCommand => new DelegateCommand(New);
        public ICommand OpenFileCommand => new DelegateCommand(OpenLoadWindow);
        public ICommand SaveFileCommand => new DelegateCommand(() => Save(true));
        public ICommand SaveAsFileCommand => new DelegateCommand(() => Save(false));
        public ICommand ExportToExcelCommand => new DelegateCommand(ExportToExcel);
        public ICommand AddTableCommand => new DelegateCommand(AddTable);

        public ICommand DeleteRowCommand => new DelegateCommand<ItemModel>(DeleteRow);
        public ICommand DeleteColumnCommand => new DelegateCommand<ColumnModel>(DeleteColumn);

        public ObservableCollection<GridColumn> GridColumns { get; set; } = new ObservableCollection<GridColumn>();
        public ObservableCollection<ColumnModel> Columns { get; set; } = new ObservableCollection<ColumnModel>();

        public MainWindow Window { get; internal set; }
        public string SelectedFile { get; internal set; }

        public void LoadData(string filePath = null)
        {
            bool flowControl = GetFileNameFromRegistryIfNull(ref filePath);
            if (!flowControl || string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(RootEntity));
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var accountingPath = System.IO.Path.Combine(appData, "Accounting");
                Window.Title = $"Účtovníctvo: {filePath}";
                SelectedFile = System.IO.Path.Combine(accountingPath, filePath);

                using (var stream = System.IO.File.OpenRead(SelectedFile))
                {
                    var root = serializer.Deserialize(stream) as RootEntity;

                    Columns.Clear();

                    if (root.Columns != null)
                    {
                        foreach (var col in root.Columns)
                        {
                            Columns.Add(CreateColumnFromData(col));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GetMessageBoxService().ShowMessage($"Niečo sa pokazilo sprav screashot a pošli Adamovy. \n {ex.Message}", "Niečo sa pokazilo", MessageButton.OK, MessageIcon.Error);
            }
        }

        private IMessageBoxService GetMessageBoxService() => GetService<IMessageBoxService>();
        private IDialogService GetSelectFileService() => GetService<IDialogService>("SelectFileService");
        private IDialogService GetSaveFileService() => GetService<IDialogService>("SaveFileService");

        private static bool GetFileNameFromRegistryIfNull(ref string fileName)
        {
            if (fileName == null)
            {
                try
                {
                    using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\FoxHint\Accounting"))
                    {
                        if (key == null) return false;

                        var regPath = key.GetValue("CsvFilePath") as string;
                        if (string.IsNullOrWhiteSpace(regPath)) return false;

                        fileName = regPath;
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        private ColumnModel CreateColumnFromData(ColumnEntity col)
        {
            var columnModel = new ColumnModel()
            {
                Tax = col.Tax
            };

            for (int i = 0; i < col.Items.Count(); i++)
            {
                var items = col.Items[i];
                var rowModel = new ItemModel(columnModel, items, col.Tax);
                columnModel.Items.Add(rowModel);
            }

            return columnModel;
        }


        private void New()
        {
            Columns.Clear();
        }

        //AppData\Roaming\Accounting\
        private void OpenLoadWindow()
        {
            var viewModel = new SelectFileViewModel();

            var okCommand = new UICommand() 
            { 
                Caption = "Vybrať",
                Command = new DelegateCommand(() => LoadData(viewModel)),
            };
            var cancelCommand = new UICommand()
            {
                Caption = "Zrušiť",
                Command = new DelegateCommand(() => { }),
                IsCancel = true
            };

            var result = GetSelectFileService().ShowDialog(new List<UICommand>() { okCommand, cancelCommand}, "Otvoriť súbor", viewModel);
        }

        private void LoadData(SelectFileViewModel viewModel)
        {
            LoadData(viewModel.SelectedFile.FileName);

            using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\FoxHint\Accounting"))
            {
                if (key == null) return;

                key.SetValue("CsvFilePath", viewModel.SelectedFile.FileName);
            }
        }

        private void Save(bool saveToExisting = false)
        {
            if (saveToExisting)
            {
                if (File.Exists(SelectedFile))
                {
                    File.Delete(SelectedFile);
                }

                var fileStream = new FileStream(SelectedFile, FileMode.CreateNew);
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(RootEntity));
                var rootEntity = new RootEntity()
                {
                    Columns = Columns.Select(c => new ColumnEntity()
                    {
                        Tax = c.Tax,
                        Items = c.Items.Where(item => item.PriceWithoutTax != null).Select(i => new ItemEntity()
                        {
                            PriceWithoutTax = i.PriceWithoutTax!.Value
                        }).ToList()
                    }).ToList()
                };
                serializer.Serialize(fileStream, rootEntity);
                return;
            }
            SaveAs();
        }

        private void SaveAs()
        {
            var viewModel = new SaveDialogViewModel(Columns.ToList());
            var okCommand = new UICommand()
            {
                Caption = "Uložiť",
                Command = new DelegateCommand(() => SaveData(viewModel)),
            };
            var cancelCommand = new UICommand()
            {
                Caption = "Zrušiť",
                Command = new DelegateCommand(() => { }),
                IsCancel = true
            };

            GetSaveFileService().ShowDialog(new List<UICommand>() { okCommand, cancelCommand }, "Uložiť súbor", viewModel);
        }

        private void SaveData(SaveDialogViewModel viewModel)
        {
            SelectedFile = viewModel.Save();
            Window.Title = $"Účtovníctvo: {System.IO.Path.GetFileName(SelectedFile)}";
        }

        private void ExportToExcel()
        {
            throw new NotImplementedException();
        }

        private void AddTable()
        {
            var columnModel = new ColumnModel()
            {

            };
            Columns.Add(columnModel);
            RaisePropertiesChanged();
        }

        private void DeleteRow(ItemModel model)
        {
            if (model == null)
            {
                return;
            }
            model.Parent.Items.Remove(model);
        }

        private void DeleteColumn(ColumnModel model)
        {
            if(model == null)
            {
                return;
            }

            Columns.Remove(model);
        }
    }
}
