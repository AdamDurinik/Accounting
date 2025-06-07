using DevExpress.Mvvm;
using PriceTags.Enums;
using PriceTags.Models;
using System.Collections.ObjectModel;

namespace PriceTags.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            SelectedTags = new();
            PriceTags.Add(new PriceTagModel { Name = "Jablko", Price = 10.99,  Quantity=60, QuantityType= QuantityType.WeightInGrams});
            PriceTags.Add(new PriceTagModel { Name = "Vajcka", Price = 5.45,  Quantity=12, QuantityType= QuantityType.Count});
            PriceTags.Add(new PriceTagModel { Name = "Muka", Price = 1.0,  Quantity=1, QuantityType= QuantityType.WeightInKilograms});
            PriceTags.Add(new PriceTagModel { Name = "Matoni", Price = 2.35,  Quantity=500, QuantityType= QuantityType.VolumeInMilliliters });
            PriceTags.Add(new PriceTagModel { Name = "Rajec", Price = 1.99,  Quantity=1.5, QuantityType= QuantityType.VolumeInLiters });
        }

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
    }
}
