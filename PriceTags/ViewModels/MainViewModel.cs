using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using DevExpress.XtraPrinting;
using PriceTags.Enums;
using PriceTags.Models;
using System.Collections.ObjectModel;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
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
            PriceTags.Add(new PriceTagModel { Name = "Jablko", Price = 10.99, Quantity = 60, QuantityType = QuantityType.WeightInGrams });
            PriceTags.Add(new PriceTagModel { Name = "Vajcka", Price = 5.45, Quantity = 12, QuantityType = QuantityType.Count });
            PriceTags.Add(new PriceTagModel { Name = "Muka", Price = 1.0, Quantity = 1, QuantityType = QuantityType.WeightInKilograms });
            PriceTags.Add(new PriceTagModel { Name = "Matoni", Price = 2.35, Quantity = 500, QuantityType = QuantityType.VolumeInMilliliters });
            PriceTags.Add(new PriceTagModel { Name = "Rajec", Price = 1.99, Quantity = 1.5, QuantityType = QuantityType.VolumeInLiters });

            Names = new ObservableCollection<string>(HintViewModel.GetNamesFromFile());
        }

        public DelegateCommand PrintCommand { get; }
        public DelegateCommand HintCommand { get; }

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

        internal void SaveNameToFile(string? currentlyEditing, string? name)
        {
            HintViewModel.AppendNameToFile(currentlyEditing, name);
            if(currentlyEditing != null && Names.Contains(currentlyEditing))
            {
                Names.Remove(currentlyEditing);
            }

            if (name != null && name != currentlyEditing)
            {
                Names.Add(name);
            }
        }

        private IDialogService GetDialogService() => GetService<IDialogService>();

        private void PrintPriceTag()
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

            var panel = new WrapPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                ItemWidth = 325,
                Margin = new Thickness(20)
            };

            foreach (var tag in SelectedTags)
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

            dlg.PrintVisual(container, "All Price Tags");
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
                ex.ToString();
            }
        }
    }
}

