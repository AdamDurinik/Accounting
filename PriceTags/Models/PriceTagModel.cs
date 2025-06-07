using DevExpress.Mvvm;
using PriceTags.Enums;

namespace PriceTags.Models
{
    public class PriceTagModel : BindableBase
    {
        public string Name
        {
            get => GetProperty(() => Name);
            set => SetProperty(() => Name, value);
        }

        public double Price
        {
            get => GetProperty(() => Price);
            set
            {
                SetProperty(() => Price, value);
                RaisePropertiesChanged();
            }
        }

        public double Quantity
        {
            get => GetProperty(() => Quantity);
            set
            {
                SetProperty(() => Quantity, value);
                RaisePropertiesChanged();
            }
        }

        public QuantityType QuantityType
        {
            get => GetProperty(() => QuantityType);
            set
            {
                SetProperty(() => QuantityType, value);
                RaisePropertiesChanged();
            }
        }

        public DateTime LastChange {
            get => GetProperty(() => LastChange);
            set => SetProperty(() => LastChange, value);
        }

        public string PricePerUnit => $"{PricePerUnitSize:n2} € / {GetUnitSize()}";

        public double PricePerUnitSize
        {
            get
            {
                if (Quantity <= 0) return 0.0;
                double unitSize = GetUnitSizeValue();
                return Price * unitSize / Quantity;
            }
        }
     
        public bool IsSale => SalePrice > 0.0;
        public bool HasDeposit => DepositAmount > 0.0;
        
        public double SalePrice
        {
            get => GetProperty(() => SalePrice);
            set
            {
                SetProperty(() => SalePrice, value);
                RaisePropertyChanged(nameof(IsSale));
            }
        }

        public double DepositAmount
        {
            get => GetProperty(() => DepositAmount);
            set
            {
                SetProperty(() => DepositAmount, value);
                RaisePropertyChanged(nameof(HasDeposit));
            }
        }

        private double GetUnitSizeValue() => QuantityType switch
        {
            QuantityType.WeightInGrams => 100.0,
            QuantityType.WeightInKilograms => 1.0,
            QuantityType.VolumeInMilliliters => 100.0,
            QuantityType.VolumeInLiters => 1.0,
            QuantityType.Count => 1.0,
            QuantityType.Packets => 1.0,
            QuantityType.Boxes => 1.0,
            _ => 1.0
        };

        private string GetUnitSize() => QuantityType switch
        {
            QuantityType.WeightInGrams => "100 g",
            QuantityType.WeightInKilograms => "1 kg",
            QuantityType.VolumeInMilliliters => "100 mil",
            QuantityType.VolumeInLiters => "1 l",
            QuantityType.Count => "1 kus",
            QuantityType.Packets => "1 balik",
            QuantityType.Boxes => "1 box",
            _ => ""
        };

    }
}
