using DevExpress.Mvvm;
using PriceTags.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace PriceTags.ViewModels
{
    public class HintViewModel : ViewModelBase
    {
        public HintViewModel()
        {
            LoadDataFromFile();

        }

        public ObservableCollection<NameModel> Names { get; set; } = new ObservableCollection<NameModel>();
        public ICommand DeleteRowCommand => new DelegateCommand<NameModel>(DeleteRow);

        private void DeleteRow(NameModel model)
        {
            if (model == null)
            {
                return;
            }
            Names.Remove(model);
        }


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

        internal static List<string> AppendNameToFile(string? currentlyEditing, string? name)
        {
            try
            {
                currentlyEditing = currentlyEditing?.Trim();
                name = name?.Trim();
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
                if (string.IsNullOrWhiteSpace(name))
                {
                    return lines;
                }
                if (currentlyEditing?.Trim() != name)
                {
                    if (!string.IsNullOrEmpty(currentlyEditing) && !string.IsNullOrEmpty(name) && lines.Contains(name))
                    {
                        lines.Remove(name);
                    }
                    if (!lines.Contains(currentlyEditing) && !string.IsNullOrEmpty(currentlyEditing))
                    {
                        lines.Add(currentlyEditing.Trim());
                    }
                    else if (!lines.Contains(name))
                    {
                        lines.Add(name);
                    }

                }

                File.WriteAllLines(filePath, lines);
                return lines;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error appending name to file: {ex.Message}");
                return new List<string>();
            }
            return null;
        }
    }
}
