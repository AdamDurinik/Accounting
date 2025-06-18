using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using PriceTags.Enums;
using PriceTags.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

namespace PriceTags.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            PrintCommand = new DelegateCommand(PrintPriceTag);
            HintCommand = new DelegateCommand(ShowHintWindow);
            SelectedTags = new();
            Names = new ObservableCollection<string>(HintViewModel.GetNamesFromFile());
            LoadItems();
        }

        public DelegateCommand PrintCommand { get; }
        public DelegateCommand HintCommand { get; }

        public ICommand DeleteRowCommand => new DelegateCommand<PriceTagModel>(RemoveItem);
        public ICommand SaveCommand => new DelegateCommand(SaveItems);

        public ObservableCollection<PriceTagModel> PriceTags { get; set; } = new ObservableCollection<PriceTagModel>();
        public ObservableCollection<string> Names
        {
            get => GetProperty(() => Names);
            set => SetProperty(() => Names, value);
        }

        public PriceTagModel SelectedPriceTag
        {
            get => GetProperty(() => SelectedPriceTag);
            set => SetProperty(() => SelectedPriceTag, value);
        }

        public List<PriceTagModel> SelectedTags
        {
            get => GetProperty(() => SelectedTags);
            set => SetProperty(() => SelectedTags, value);
        }

        public void SaveItems()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var accountingPath = System.IO.Path.Combine(appData, "Accounting");

            if (!Directory.Exists(accountingPath))
            {
                Directory.CreateDirectory(accountingPath);
            }
            var fileName = "PriceTags";
            var filePath = Path.Combine(accountingPath, fileName);
            var fullFilePath = filePath + ".xaml";

            using (var writer = new StreamWriter(fullFilePath, false))
            {
                foreach (var tag in PriceTags)
                {
                    try
                    {
                        string csvLine = "";
                        csvLine += tag.Name?.Replace("\"", "\"\"");
                        csvLine += ",";
                        csvLine += tag.Price.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        csvLine += ",";
                        csvLine += tag.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        csvLine += ",";
                        csvLine += tag.QuantityType.ToString();
                        csvLine += ",";
                        csvLine += tag.SalePrice.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        csvLine += ",";
                        csvLine += tag.DepositAmount.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        // Add additional fields if needed, separated by commas
                        writer.WriteLine(csvLine);
                    }
                    catch (Exception ex)
                    {
                        GetMessageBoxService()?.ShowMessage("Niečo sa stalo pri ukladaní položky. Urob screenshot a pošli Adamkovi.\n\n" + ex.Message, "Chyba", MessageButton.OK, MessageIcon.Error);
                    }
                }
            }
        }

        internal void SaveNameToFile(string? currentlyEditing, string? name)
        {
            try
            {
                var lines = HintViewModel.AppendNameToFile(currentlyEditing, name);
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Names.Clear();
                    lines.Select(l => new NameModel(l)).ForEach(n => Names.Add(n.Name));
                });
            }
            catch (Exception ex)
            {
                GetMessageBoxService()?.ShowMessage("Niečo sa stalo pri ukladaní mena. Urob screenshot a pošli Adamkovi.\n\n" + ex.Message, "Chyba", MessageButton.OK, MessageIcon.Error);
            }
        }

        private IMessageBoxService GetMessageBoxService() => GetService<IMessageBoxService>();
        private IDialogService GetDialogService() => GetService<IDialogService>();

        private void PrintPriceTag()
        {
            try
            {
                var dlg = new System.Windows.Controls.PrintDialog
                {
                    PrintTicket = { PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4) }
                };
                if (dlg.ShowDialog() != true) return;

                double printableW = dlg.PrintableAreaWidth;
                double printableH = dlg.PrintableAreaHeight;
                var mainView = (FrameworkElement)System.Windows.Application.Current.MainWindow.Content;
                var template = (DataTemplate)mainView.Resources["PriceTagPrintTemplate"];

                const double itemWidth = 325;
                const double itemHeight = 200;
                const double margin = 20;

                int itemsPerRow = (int)((printableW - margin * 2) / itemWidth);
                int rowsPerPage = (int)((printableH - margin * 2) / itemHeight);
                int itemsPerPage = itemsPerRow * rowsPerPage+2;

                var document = new FixedDocument();
                document.DocumentPaginator.PageSize = new System.Windows.Size(printableW, printableH);

                var items = SelectedTags.Where(s => !string.IsNullOrEmpty(s.Name)).ToList();


                for (int i = 0; i < items.Count(); i += itemsPerPage)
                {
                    var panel = new WrapPanel
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        ItemWidth = itemWidth,
                        Margin = new Thickness(margin)
                    };

                    foreach (var tag in items.Skip(i).Take(itemsPerPage))
                    {
                        var content = (FrameworkElement)template.LoadContent();
                        content.DataContext = tag;
                        panel.Children.Add(content);
                    }

                    var container = new Grid
                    {
                        Width = printableW,
                        Height = printableH
                    };
                    container.Children.Add(panel);

                    container.Measure(new System.Windows.Size(printableW, printableH));
                    container.Arrange(new Rect(0, 0, printableW, printableH));
                    container.UpdateLayout();

                    var page = new FixedPage
                    {
                        Width = printableW,
                        Height = printableH
                    };
                    page.Children.Add(container);

                    var pageContent = new PageContent();
                    ((IAddChild)pageContent).AddChild(page);
                    document.Pages.Add(pageContent);
                }

                dlg.PrintDocument(document.DocumentPaginator, "Všetky cenovky");
            }
            catch (Exception ex)
            {
                GetMessageBoxService()?.ShowMessage("Niečo sa stalo pri tlači cenoviek. Urob screenshot a pošli Adamkovi.\n\n" + ex.Message, "Chyba", MessageButton.OK, MessageIcon.Error);
            }
        }

        private void ShowHintWindow()
        {
            var viewModel = new HintViewModel();

            var okCommand = new UICommand()
            {
                Caption = "Uložiť",
                Command = new DelegateCommand(() => viewModel.SaveDataToFile()),
            };
            var cancelCommand = new UICommand()
            {
                Caption = "Zatvoriť",
                Command = new DelegateCommand(() => viewModel.SaveDataToFile()),
                IsCancel = true
            };

            try
            {
                GetDialogService().ShowDialog(new List<UICommand>() { okCommand, cancelCommand }, "Nápovedy Názvou", viewModel);
                Names.Clear();
                viewModel.Names.Select(n => n.Name).ForEach(n => Names.Add(n));
            }
            catch (Exception ex)
            {
                GetMessageBoxService()?.ShowMessage("Niečo sa stalo pri zobrazovaní okna nápovedy. Urob screenshot a pošli Adamkovi.\n\n" + ex.Message, "Chyba", MessageButton.OK, MessageIcon.Error);
            }
        }

        private void RemoveItem(PriceTagModel model)
        {
            try
            {
                if (model != null && PriceTags.Contains(model))
                {
                    PriceTags.Remove(model);
                }
            }
            catch (Exception ex)
            {
                GetMessageBoxService()?.ShowMessage("Niečo sa stalo pri odstraňovaní položky. Urob screenshot a pošli Adamkovi.\n\n" + ex.Message, "Chyba", MessageButton.OK, MessageIcon.Error);
            }
        }

        public void LoadItems()
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var accountingPath = System.IO.Path.Combine(appData, "Accounting");
                var fileName = "PriceTags";
                var filePath = Path.Combine(accountingPath, fileName);
                var fullFilePath = filePath + ".xaml";

                if (!File.Exists(fullFilePath))
                    return;

                PriceTags.Clear();

                var lines = File.ReadAllLines(fullFilePath);
                foreach (var line in lines)
                {
                    var fields = new List<string>();
                    bool inQuotes = false;
                    var field = new System.Text.StringBuilder();
                    for (int i = 0; i < line.Length; i++)
                    {
                        char c = line[i];
                        if (c == '\"')
                        {
                            if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                            {
                                field.Append('\"');
                                i++;
                            }
                            else
                            {
                                inQuotes = !inQuotes;
                            }
                        }
                        else if (c == ',' && !inQuotes)
                        {
                            fields.Add(field.ToString());
                            field.Clear();
                        }
                        else
                        {
                            field.Append(c);
                        }
                    }
                    fields.Add(field.ToString());

                    if (fields.Count < 5)
                        continue;

                    try
                    {
                        var model = new PriceTagModel
                        {
                            Name = fields[0],
                            Price = double.TryParse(fields[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var price) ? price : 0,
                            Quantity = double.TryParse(fields[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var qty) ? qty : 0,
                            QuantityType = Enum.TryParse<QuantityType>(fields[3], out var qt) ? qt : QuantityType.Count,
                            SalePrice = double.TryParse(fields[4], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var sp) ? sp : 0,
                            DepositAmount = double.TryParse(fields[5], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var da) ? da : 0
                        };
                        PriceTags.Add(model);
                    }
                    catch
                    {
                        // Ignore malformed lines
                    }
                }
            }
            catch (Exception ex)
            {
                GetMessageBoxService()?.ShowMessage("Niečo sa stalo pri načítavaní položiek. Urob screenshot a pošli Adamkovi.\n\n" + ex.Message, "Chyba", MessageButton.OK, MessageIcon.Error);
            }
        }
    }
}

