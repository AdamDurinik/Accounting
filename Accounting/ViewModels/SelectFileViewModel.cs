using Accounting.Models;
using DevExpress.Mvvm;
using System.Collections.ObjectModel;

namespace Accounting.ViewModels
{
    public class SelectFileViewModel : ViewModelBase
    {
        public SelectFileViewModel()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var accountingPath = System.IO.Path.Combine(appData, "Accounting");

            if (!System.IO.Directory.Exists(accountingPath))
            {
                System.IO.Directory.CreateDirectory(accountingPath);
            }

            var files = System.IO.Directory.GetFiles(accountingPath);
            foreach (var file in files)
            {
                var fileInfo = new System.IO.FileInfo(file);
                FileList.Add(new FileModel
                {
                    FileName = fileInfo.Name,
                    CreatedDate = fileInfo.CreationTime,
                    LastModifiedDate = fileInfo.LastWriteTime
                });
            }
        }

        public ObservableCollection<FileModel> FileList { get; set; } = new ObservableCollection<FileModel>();

        public FileModel SelectedFile 
        {
            get => GetProperty(() => SelectedFile);
            set => SetProperty(() => SelectedFile, value);
        }

    }
}
