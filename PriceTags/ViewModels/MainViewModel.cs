using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using PriceTags.Enums;
using PriceTags.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Velopack;
using Velopack.Sources;

namespace PriceTags.ViewModels
{
    public class MainViewModel : ViewModelBase
    {

        private const int MaxItemsPerPage = 21;
        private DispatcherTimer? _autoSaveTimer;

        public MainViewModel()
        {
            SelectedTags = new();
            PrintCommand = new DelegateCommand(PrintPriceTag, () => SelectedTags.Any());
            HintCommand = new DelegateCommand(ShowHintWindow);
            DeleteSelectedCommand = new DelegateCommand(DeleteSelected, () => SelectedTags.Any());
            UpdateApplicationCommand = new DelegateCommand(() => UpdateApplication());
            Names = new ObservableCollection<string>(HintViewModel.GetNamesFromFile());
            LoadItems();

            try
            {
                if (HasSavedFile())
                {
                    StartAutoSaveTimer();
                }
            }
            catch
            {
                // Ignore errors starting the timer
            }
        }

        public bool IsUpdateAvailable
        {
            get => GetProperty(() => IsUpdateAvailable);
            set => SetProperty(() => IsUpdateAvailable, value);
        }

        public string CurrentVersion
        {
            get => GetProperty(() => CurrentVersion) ?? "Neznama verzia";
            set => SetProperty(() => CurrentVersion, value);
        }

        public DelegateCommand PrintCommand { get; }
        public DelegateCommand HintCommand { get; }
        public DelegateCommand DeleteSelectedCommand { get; }
        public DelegateCommand UpdateApplicationCommand { get; }

        public ICommand DeleteRowCommand => new DelegateCommand<PriceTagModel>(RemoveItem);
        public ICommand SaveCommand => new DelegateCommand(SaveItems);

        public bool ShowImage
        {
            get => GetProperty(() => ShowImage);
            set
            {
                SetProperty(() => ShowImage, value);
                PriceTags.ForEach(p => p.ShowImage = value);
            }
        }

        public string PageCountAndTagCount
        {
            get => GetProperty(() => PageCountAndTagCount);
            set => SetProperty(() => PageCountAndTagCount, value);
        }

        public string GetNewPageCountTagCount() 
        {
            if (!SelectedTags.Any())
            {
                return string.Empty;
            }

            var numberOfPages = (SelectedTags.Count() / MaxItemsPerPage) + (SelectedTags.Count() % MaxItemsPerPage == 0 ? 0 : 1);
            return $"Počet stránok {numberOfPages} / cenovky {SelectedTags.Count()} / {numberOfPages * MaxItemsPerPage}";
        }

        public ObservableCollection<PriceTagModel> PriceTags { get; set; } = new();
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
            set
            {
                try
                {
                    SetProperty(() => SelectedTags, value);
                }
                catch (Exception ex)
                {
                    GetMessageBoxService()?.ShowMessage("Niečo sa stalo pri nastavovaní vybraných položiek. Urob screenshot a pošli Adamkovi.\n\n" + ex.Message, "Chyba", MessageButton.OK, MessageIcon.Error);
                }
            }
        }

        public string UpdateText
        {
            get => GetProperty(() => UpdateText);
            set => SetProperty(() => UpdateText, value);
        }

        public void SaveItems()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var accountingPath = Path.Combine(appData, "Accounting");

            if (!Directory.Exists(accountingPath))
            {
                Directory.CreateDirectory(accountingPath);
            }
            var fileName = "PriceTags";
            var filePath = Path.Combine(accountingPath, fileName);
            var fullFilePath = filePath + ".xaml";

            using var writer = new StreamWriter(fullFilePath, false);
            foreach (var tag in PriceTags)
            {
                try
                {
                    var csvLine = "";
                    csvLine += tag.Name?.Replace("\"", "\"\"");
                    csvLine += ",";
                    csvLine += tag.Price.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    csvLine += ",";
                    csvLine += tag.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    csvLine += ",";
                    csvLine += tag.QuantityType.ToString();
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

        private void DeleteSelected()
        {
            foreach (var tag in SelectedTags.ToList())
            {
                RemoveItem(tag);
            }
            SelectedTags.Clear();
        }

        private IMessageBoxService GetMessageBoxService() => GetService<IMessageBoxService>();
        private IDialogService GetDialogService() => GetService<IDialogService>();

        private void PrintPriceTag()
        {
            try
            {
                SaveItems();
                var dlg = new System.Windows.Controls.PrintDialog
                {
                    PrintTicket = { PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4) }
                };
                if (dlg.ShowDialog() != true) return;

                var printableW = dlg.PrintableAreaWidth;
                var printableH = dlg.PrintableAreaHeight;

                const double pageMargin = 10;
                var availableW = Math.Max(0, printableW - pageMargin * 2);
                var availableH = Math.Max(0, printableH - pageMargin * 2);

                var mainView = (FrameworkElement)System.Windows.Application.Current.MainWindow.Content;
                var template = (DataTemplate)mainView.Resources["PriceTagPrintTemplate"];

                const double itemWidth = 250;
                const double itemHeight = 145;

                var itemsPerRow = (int)(availableW / itemWidth);
                var rowsPerPage = (int)(availableH / itemHeight);

                // Ensure at least one item per row / one row per page to avoid division by zero.
                itemsPerRow = Math.Max(1, itemsPerRow);
                rowsPerPage = Math.Max(1, rowsPerPage);

                var itemsPerPage = itemsPerRow * rowsPerPage;

                var document = new FixedDocument();
                document.DocumentPaginator.PageSize = new System.Windows.Size(printableW, printableH);

                var items = SelectedTags.Where(s => !string.IsNullOrEmpty(s.Name)).ToList();


                for (var i = 0; i < items.Count(); i += itemsPerPage)
                {
                    var panel = new WrapPanel
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        ItemWidth = itemWidth,
                        Margin = new Thickness(pageMargin) // inset content from all printable edges
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
                GetMessageBoxService()?.ShowMessage("Niečo sa stalo pri tlači cenoviek. \n Asi nieje nič vybrané, ak áno; urob screenshot a pošli Adamkovi.\n\n" + ex.Message, "Chyba", MessageButton.OK, MessageIcon.Error);
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
                    var inQuotes = false;
                    var field = new System.Text.StringBuilder();
                    for (var i = 0; i < line.Length; i++)
                    {
                        var c = line[i];
                        switch (c)
                        {
                            case '\"' when inQuotes && i + 1 < line.Length && line[i + 1] == '\"':
                                field.Append('\"');
                                i++;
                                break;
                            case '\"':
                                inQuotes = !inQuotes;
                                break;
                            case ',' when !inQuotes:
                                fields.Add(field.ToString());
                                field.Clear();
                                break;
                            default:
                                field.Append(c);
                                break;
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
                            DepositAmount = double.TryParse(fields[4], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var da) ? da : 0
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

        private void StartAutoSaveTimer()
        {
            if (_autoSaveTimer != null) return;

            _autoSaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _autoSaveTimer.Tick += AutoSaveTimer_Tick;
            _autoSaveTimer.Start();
      
        }

        private async Task CheckForUpdates()
        {
            try
            {
                var updateUrl = "https://pricetags.foxhint.com/updates/";
                var mgr = new UpdateManager(new SimpleWebSource(updateUrl));
                CurrentVersion = mgr?.CurrentVersion?.ToString() ?? "Neznáma verzia";

                var newVersion = await mgr?.CheckForUpdatesAsync();
                IsUpdateAvailable = newVersion != null;
                UpdateText = IsUpdateAvailable ? $"Nová verzia {newVersion.TargetFullRelease.Version} je dostupná na stiahnutie." : "Aplikácia je aktuálna.";
            }
            catch(Exception ex)
            {
                UpdateText = ex.Message;

            }
        }

        private void AutoSaveTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (HasSavedFile())
                {
                    SaveItems();
                    Task.Run(CheckForUpdates); 
                }
            }
            catch
            {
                // Silently ignore autosave errors to avoid disturbing the user
            }
        }

        private bool HasSavedFile()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var accountingPath = Path.Combine(appData, "Accounting");
            var fileName = "PriceTags";
            var filePath = Path.Combine(accountingPath, fileName);
            var fullFilePath = filePath + ".xaml";
            return File.Exists(fullFilePath);
        }

        private async Task UpdateApplication()
        {
            try
            {
                var updateUrl = $"https://pricetags.foxhint.com/updates/?t={DateTime.Now.Ticks}";
                var mgr = new UpdateManager(new SimpleWebSource(updateUrl));

                var newVersion = await mgr.CheckForUpdatesAsync();
                if (newVersion == null)
                {
                    UpdateText = "Aplikácia je aktuálna."; 
                    return; 
                }

                UpdateText = $"Sťahovanie verzie {newVersion.TargetFullRelease.Version}...";

                await mgr.DownloadUpdatesAsync(newVersion, progress =>
                {
                    UpdateText = $"Sťahovanie: {progress}%";
                });

                UpdateText = "Aktualizácia pripravená. Reštartujem..."; 

                await Task.Delay(1000);

                mgr.ApplyUpdatesAndRestart(newVersion);
            }
            catch (Exception ex)
            {
                GetMessageBoxService()?.ShowMessage(
                    "Chyba pri aktualizácii: " + ex.Message,
                    "Chyba",
                    MessageButton.OK,
                    MessageIcon.Error);

                UpdateText = "Chyba pri sťahovaní.";
            }
        }
    }
    
}

