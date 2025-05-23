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
                    // compute tax on new base price
                    Tax = RoundUp(value * TaxValue);
                    // compute total
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
                        // recompute base price from new tax
                        PriceWithoutTax = RoundUp(value.Value / TaxValue);
                        // recompute total
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
                    // recompute tax from new total
                    Tax = RoundUp((value * TaxValue) / (1 + TaxValue));
                    // recompute base
                    PriceWithoutTax = RoundUp((value ?? 0) - (Tax ?? 0));
                    _isUpdating = false;
                }
            }
        }

        public void UpdateWithTax(int taxPercent)
        {
            try
            {
                TaxValue = taxPercent / 100.0;
                // Kick off a full recalculation
                RecalculateFromPrice();

            }
            catch
            {

            }
        }

        private void RecalculateFromPrice()
        {
            if (PriceWithoutTax.HasValue)
            {
                _isUpdating = true;
                Tax = RoundUp(PriceWithoutTax * TaxValue);
                PriceWithTax = RoundUp((PriceWithoutTax ?? 0) + (Tax ?? 0));
                _isUpdating = false;
            }
        }

        /// <summary>
        /// Rounds up (ceiling) to 2 decimal places.
        /// </summary>
        private double? RoundUp(double? value)
        {
            if (!value.HasValue) return null;
            // Multiply, ceiling, then divide
            return Math.Ceiling(value.Value * 100) / 100.0;
        }
    }
}
