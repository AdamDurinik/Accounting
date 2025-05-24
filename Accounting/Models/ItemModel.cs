using Accounting.Entity;
using DevExpress.Mvvm;

namespace Accounting.Models
{
    public class ItemModel : BindableBase
    {
        bool _isUpdating = false;
        private ColumnModel column;

        public ItemModel(ColumnModel parent, ItemEntity item, double taxPercent)
        {
            Parent = parent;
            PriceWithoutTax = item.PriceWithoutTax;
            TaxValue = taxPercent / 100.0;  // e.g. 10% → 0.10
            UpdateWithTax((int)taxPercent);
        }

        public ItemModel()
        {
            
        }

        public ItemModel(ColumnModel column)
        {
            Parent = column;
            TaxValue = column.Tax;
        }

        public ColumnModel Parent { get; set; }

        private double TaxValue
        {
            get => GetProperty(() => TaxValue);
            set => SetProperty(() => TaxValue, value);
        }

        public double? PriceWithoutTax
        {
            get => GetProperty(() => PriceWithoutTax);
            set
            {
                if (SetProperty(() => PriceWithoutTax, RoundUp(value)) && !_isUpdating)
                {
                    _isUpdating = true;
                    Tax = RoundUp(value * (TaxValue/100.0));
                    PriceWithTax = RoundUp(value + (Tax ?? 0));
                    _isUpdating = false;
                }
            }
        }

        public double? Tax
        {
            get => GetProperty(() => Tax);
            set
            {
                if (SetProperty(() => Tax, RoundUp(value)) && !_isUpdating)
                {
                    if (TaxValue > 0 && value.HasValue)
                    {
                        _isUpdating = true;
                        PriceWithoutTax = RoundUp(value.Value / (TaxValue/100.0));
                        PriceWithTax = RoundUp((PriceWithoutTax ?? 0) + value.Value);
                        _isUpdating = false;
                    }
                }
            }
        }

        public double? PriceWithTax
        {
            get => GetProperty(() => PriceWithTax);
            set
            {
                if (SetProperty(() => PriceWithTax, RoundUp(value)) && !_isUpdating)
                {
                    _isUpdating = true;
                    Tax = RoundUp((value * TaxValue / 100.0) / (1 + (TaxValue/100.0)));
                    PriceWithoutTax = RoundUp((value ?? 0) - (Tax ?? 0));
                    _isUpdating = false;
                }
            }
        }

        public void UpdateWithTax(int taxPercent)
        {
            TaxValue = taxPercent;
            RecalculateFromPrice();
        }

        private void RecalculateFromPrice()
        {
            if (PriceWithoutTax.HasValue)
            {
                _isUpdating = true;
                Tax = RoundUp(PriceWithoutTax * TaxValue / 100.0);
                PriceWithTax = RoundUp((PriceWithoutTax ?? 0) + (Tax ?? 0));
                _isUpdating = false;
            }
        }

        private double? RoundUp(double? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return Math.Ceiling(value.Value * 100) / 100.0;
        }
    }
}
