using Accounting.Entity;
using Accounting.Models;
using DevExpress.Mvvm;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;

namespace Accounting.ViewModels
{
    public class SaveDialogViewModel : ViewModelBase, IDataErrorInfo
    {
        private List<ColumnModel> _columns;

        public SaveDialogViewModel(List<ColumnModel> columns)
        {
            _columns = columns;
        }

        public string FileName
        {
            get => GetProperty(() => FileName);
            set => SetProperty(() => FileName, value);
        }

        public string Error => string.Empty;

        public string this[string columnName] => columnName switch
        {
            nameof(FileName) when !FileNameDoesNotExist() => "Názov súboru je neplatný alebo už existuje.",
            nameof(FileName) when !IsValidFileName() => "Názov súboru je neplatný",
            _ => string.Empty,
        };

        public string Save()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var accountingPath = System.IO.Path.Combine(appData, "Accounting");

            if(!Directory.Exists(accountingPath))
            {
                Directory.CreateDirectory(accountingPath);
            }

            var filePath = System.IO.Path.Combine(accountingPath, FileName);
            var fileStream = File.Create(filePath + ".xaml");

            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(RootEntity));
            var rootEntity = new RootEntity()
            {
                Columns = _columns.Select(c => new ColumnEntity()
                {
                    Tax = c.Tax,
                    Items = c.Items.Where(item => item.PriceWithoutTax != null).Select(i => new ItemEntity()
                    {
                        PriceWithoutTax = i.PriceWithoutTax!.Value
                    }).ToList()
                }).ToList()
            };

            serializer.Serialize(fileStream, rootEntity);

            return FileName;
        }


        public bool CanSaveFile() => !string.IsNullOrWhiteSpace(FileName);

        private bool FileNameDoesNotExist()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var accountingPath = System.IO.Path.Combine(appData, "Accounting");
            var files = Directory.GetFiles(accountingPath);

            return files.All(f => !Path.GetFileNameWithoutExtension(f).Equals(FileName, StringComparison.OrdinalIgnoreCase)) &&
                   !string.IsNullOrWhiteSpace(FileName) &&
                   FileName.Length <= 50;
        }

        private bool IsValidFileName()
        {
            if (string.IsNullOrWhiteSpace(FileName)) return false;
            if (FileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return false;
            if (FileName.EndsWith(" ") || FileName.EndsWith(".")) return false;
            string[] reserved = {
                "CON","PRN","AUX","NUL",
                "COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8","COM9",
                "LPT1","LPT2","LPT3","LPT4","LPT5","LPT6","LPT7","LPT8","LPT9"
            };
            var nameOnly = Path.GetFileNameWithoutExtension(FileName).ToUpperInvariant();
            if (reserved.Contains(nameOnly)) return false;
            return true;
        }
    }
}
