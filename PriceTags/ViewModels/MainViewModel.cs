using DevExpress.Mvvm;
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

            SelectedTags = new();
            PriceTags.Add(new PriceTagModel { Name = "Jablko", Price = 10.99,  Quantity=60, QuantityType= QuantityType.WeightInGrams});
            PriceTags.Add(new PriceTagModel { Name = "Vajcka", Price = 5.45,  Quantity=12, QuantityType= QuantityType.Count});
            PriceTags.Add(new PriceTagModel { Name = "Muka", Price = 1.0,  Quantity=1, QuantityType= QuantityType.WeightInKilograms});
            PriceTags.Add(new PriceTagModel { Name = "Matoni", Price = 2.35,  Quantity=500, QuantityType= QuantityType.VolumeInMilliliters });
            PriceTags.Add(new PriceTagModel { Name = "Rajec", Price = 1.99,  Quantity=1.5, QuantityType= QuantityType.VolumeInLiters });
        }

        public DelegateCommand PrintCommand { get; }

        public ObservableCollection<PriceTagModel> PriceTags { get; set; } = new ObservableCollection<PriceTagModel>();
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

        void PrintPriceTag()
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
                ItemWidth = 330,
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

    }
}

