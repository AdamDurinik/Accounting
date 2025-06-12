using Accounting.Entity;
using Accounting.Exporters;
using Accounting.Models;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Xpf;
using DevExpress.Xpf.Grid;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace Accounting.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
        }

        public ICommand CreateNewFileCommand => new DelegateCommand(New);
        public ICommand OpenFileCommand => new DelegateCommand(OpenLoadWindow);
        public ICommand SaveFileCommand => new DelegateCommand(() => Save(true), () => !string.IsNullOrEmpty(SelectedFile));
        public ICommand SaveAsFileCommand => new DelegateCommand(() => Save(false), () => !string.IsNullOrEmpty(SelectedFile));
        public ICommand ExportToExcelCommand => new DelegateCommand(ExportToExcel, () => !string.IsNullOrEmpty(SelectedFile));
        public ICommand AddTableCommand => new DelegateCommand(AddTable, () => !string.IsNullOrEmpty(SelectedFile));

        public ICommand DeleteRowCommand => new DelegateCommand<ItemModel>(DeleteRow);
        public ICommand DeleteColumnCommand => new DelegateCommand<ColumnModel>(DeleteColumn);

        public ObservableCollection<GridColumn> GridColumns { get; set; } = new ObservableCollection<GridColumn>();
        public ObservableCollection<ColumnModel> Columns { get; set; } = new ObservableCollection<ColumnModel>();

        public MainWindow Window { get; internal set; }
        public string SelectedFile { get; internal set; }

        public bool LoadData(string fileName = null)
        {
            bool flowControl = GetFileNameFromRegistryIfNull(ref fileName);
            if (!flowControl || string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            try
            {
                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(RootEntity));
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var accountingPath = System.IO.Path.Combine(appData, "Accounting");
                if (!fileName.Contains(".xaml"))
                {
                    fileName = fileName + ".xaml";
                }
                SelectedFile = System.IO.Path.GetFileNameWithoutExtension(fileName);
                var filaPath = System.IO.Path.Combine(accountingPath, fileName);

                if (!File.Exists(filaPath))
                {
                    return false;
                }
                Window.Title = $"Účtovníctvo: {SelectedFile}";

                using (var stream = System.IO.File.OpenRead(filaPath))
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
                GetMessageBoxService().ShowMessage($"Niečo sa pokazilo sprav screashot a pošli Adamovi. \n {ex.Message}", "Niečo sa pokazilo", MessageButton.OK, MessageIcon.Error);
                return false;
            }

            return true;
        }

        public void Save(bool saveToExisting = false)
        {
            SaveToRegistry(SelectedFile);
            if (saveToExisting)
            {
                var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Accounting", SelectedFile + ".xaml");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                using var fileStream = new FileStream(filePath, FileMode.CreateNew);
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

        private IMessageBoxService GetMessageBoxService() => GetService<IMessageBoxService>();
        private IDialogService GetSelectFileService() => GetService<IDialogService>("SelectFileService");
        private IDialogService GetSaveFileService() => GetService<IDialogService>("SaveFileService");
        private ISaveFileDialogService GetSaveDialogService() => GetService<ISaveFileDialogService>("SaveFileDialogService");

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
            var viewModel = new SaveDialogViewModel(Columns.ToList());
            var okCommand = new UICommand()
            {
                Caption = "Pridať",
                Command = new DelegateCommand(() => CreateNewTable(viewModel), () => !string.IsNullOrEmpty(viewModel.FileName)),
                IsDefault = true
            };
            var cancelCommand = new UICommand()
            {
                Caption = "Zrušiť",
                Command = new DelegateCommand(() => { }),
                IsCancel = true
            };

            var service = GetSaveFileService();
            if (service == null)
            {
                GetMessageBoxService().ShowMessage("Niečo sa pokazilo, zavolaj Adamovi. \n Nenašla sa SaveFileService.", "Niečo sa pokazilo", MessageButton.OK, MessageIcon.Error);
            }
            service?.ShowDialog(new List<UICommand>() { okCommand, cancelCommand }, "Pridať súbor", viewModel);
        }

        private void CreateNewTable(SaveDialogViewModel viewModel)
        {
            Columns.Clear();

            Columns.Add(new ColumnModel()
            {
                Tax = 20,
            });
            Columns.Add(new ColumnModel()
            {
                Tax = 19,
            });
            Columns.Add(new ColumnModel()
            {
                Tax = 5,
            });

            viewModel.Save();
            Window.Title = $"Účtovníctvo: {System.IO.Path.GetFileName(viewModel.FileName)}";
            SelectedFile = viewModel.FileName;
            SaveToRegistry(viewModel.FileName);
        }

        private void OpenLoadWindow()
        {
            var viewModel = new SelectFileViewModel();

            var okCommand = new UICommand()
            {
                Caption = "Vybrať",
                Command = new DelegateCommand(() => LoadData(viewModel), () => viewModel.SelectedFile != null),
                IsDefault = true
            };
            var cancelCommand = new UICommand()
            {
                Caption = "Zrušiť",
                Command = new DelegateCommand(() => { }),
                IsCancel = true
            };

            viewModel.OkCommand = okCommand;
            try
            {
                var service = GetSelectFileService();
                var result = GetSelectFileService().ShowDialog(new List<UICommand>() { okCommand, cancelCommand }, "Otvoriť súbor", viewModel);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private void LoadData(SelectFileViewModel viewModel)
        {
            if (!LoadData(viewModel.SelectedFile.FileName))
            {
                return;
            }

            bool flowControl = SaveToRegistry(viewModel.SelectedFile.FileName);
            if (!flowControl)
            {
                return;
            }
        }

        private static bool SaveToRegistry(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }
            using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"SOFTWARE\FoxHint\Accounting"))
            {
                if (key == null) return false;

                key.SetValue("CsvFilePath", fileName);
            }

            return true;
        }

        private void SaveAs()
        {
            var viewModel = new SaveDialogViewModel(Columns.ToList());
            var okCommand = new UICommand()
            {
                Caption = "Uložiť",
                Command = new DelegateCommand(() => SaveData(viewModel), () => viewModel.CanSaveFile()),
                IsDefault = true,
            };
            var cancelCommand = new UICommand()
            {
                Caption = "Zrušiť",
                Command = new DelegateCommand(() => { }),
                IsCancel = true
            };

            var service = GetSaveFileService();
            if(service == null)
            {
                GetMessageBoxService().ShowMessage("Niečo sa pokazilo, zavolaj Adamovi. \n Nenašla sa SaveFileService.", "Niečo sa pokazilo", MessageButton.OK, MessageIcon.Error);
            }
            service?.ShowDialog(new List<UICommand>() { okCommand, cancelCommand }, "Uložiť súbor", viewModel);
        }

        private void SaveData(SaveDialogViewModel viewModel)
        {
            SelectedFile = viewModel.Save();
            Window.Title = $"Účtovníctvo: {System.IO.Path.GetFileName(SelectedFile)}";
        }

        private void ExportToExcel()
        {
            var service = GetSaveDialogService();
            service.DefaultFileName = SelectedFile;
            if (service != null && service.ShowDialog())
            {
                var fileInfo = service.File.Name;
                var directory = service.File.DirectoryName;
                var filePath = System.IO.Path.Combine(directory, fileInfo);
                var exporter = new ExcelExporter();
                exporter.Export(filePath, Columns.ToList());

            }
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
