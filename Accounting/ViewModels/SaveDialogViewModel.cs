using Accounting.Entity;
using Accounting.Models;
using DevExpress.Mvvm;
using System.IO;
using System.Windows.Input;

namespace Accounting.ViewModels
{
    public class SaveDialogViewModel : ViewModelBase
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

        public string Save()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var accountingPath = System.IO.Path.Combine(appData, "Accounting");
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

        private void Cancel()
        {
            throw new NotImplementedException();
        }
    }
}
