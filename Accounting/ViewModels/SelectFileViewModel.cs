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
            var fileModels = new List<FileModel>();
            foreach (var file in files)
            {
                var fileInfo = new System.IO.FileInfo(file);
                fileModels.Add(new FileModel
                {
                    FileName = System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name),
                    CreatedDate = fileInfo.CreationTime,
                    LastModifiedDate = fileInfo.LastWriteTime
                });
            }

            fileModels.OrderByDescending(f => f.CreatedDate).ToList().ForEach(FileList.Add);
        }

        public ObservableCollection<FileModel> FileList { get; set; } = new ObservableCollection<FileModel>();

        public FileModel SelectedFile 
        {
            get => GetProperty(() => SelectedFile);
            set => SetProperty(() => SelectedFile, value);
        }
        public UICommand OkCommand { get; internal set; }
    }
}
