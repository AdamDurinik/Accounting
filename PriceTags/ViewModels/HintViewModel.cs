using DevExpress.Mvvm;
using PriceTags.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace PriceTags.ViewModels
{
    public class HintViewModel : ViewModelBase
    {
        public HintViewModel()
        {
            LoadDataFromFile();
        }

        public ObservableCollection<NameModel> Names { get; set; } = new ObservableCollection<NameModel>();

        private void LoadDataFromFile()
        {
            string tempPath = Path.GetTempPath();
            string folderPath = Path.Combine(tempPath, "PriceTags");
            string filePath = Path.Combine(folderPath, "NamesForPriceTags.txt");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
                return;
            }

            var lines = File.ReadAllLines(filePath);
            Names.Clear();
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    Names.Add(new NameModel(line.Trim()));
                }
            }
        }

        public void SaveDataToFile()
        {
            string tempPath = Path.GetTempPath();
            string folderPath = Path.Combine(tempPath, "PriceTags");
            string filePath = Path.Combine(folderPath, "NamesForPriceTags.txt");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                foreach (var name in Names)
                {
                    writer.WriteLine(name.Name);
                }
            }
        }


        public static IEnumerable<string> GetNamesFromFile()
        {
            string tempPath = Path.GetTempPath();
            string folderPath = Path.Combine(tempPath, "PriceTags");
            string filePath = Path.Combine(folderPath, "NamesForPriceTags.txt");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }
            else
            {

                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        yield return line.Trim();
                    }
                }
            }
        }

        internal static void AppendNameToFile(string? currentlyEditing, string? name)
        {
            if (string.IsNullOrWhiteSpace(name) || currentlyEditing == name)
            {
                return;
            }
            string tempPath = Path.GetTempPath();
            string folderPath = Path.Combine(tempPath, "PriceTags");
            string filePath = Path.Combine(folderPath, "NamesForPriceTags.txt");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }
            var lines = File.ReadAllLines(filePath).ToList();
            if (currentlyEditing != null)
            {
                lines.Remove(currentlyEditing);
            }
            lines.Add(name.Trim());
            
            File.WriteAllLines(filePath, lines);

        }
    }
}
