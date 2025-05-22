using Accounting.Entity;
using DevExpress.Mvvm;
using System;

namespace Accounting.Models
{
    public class ItemModel : BindableBase
    {
        bool _isUpdating = false;

        public ItemModel(ItemEntity item, double taxPercent)
        {
            PriceWithoutTax = item.PriceWithoutTax;
            TaxValue = taxPercent / 100.0;  // e.g. 10% → 0.10
            UpdateWithTax((int)(taxPercent));
        }
        public ItemModel() { }

        public void UpdateWithTax(int taxPercent)
        {
            TaxValue = taxPercent / 100.0;
            // Kick off a full recalculation
            RecalculateFromPrice();
        }

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
                if (SetProperty(() => PriceWithoutTax, value) && !_isUpdating)
                {
                    _isUpdating = true;
                    // compute tax on new base price
                    Tax = value * TaxValue;
                    // compute total
                    PriceWithTax = value + Tax;
                    _isUpdating = false;
                }
            }
        }

        public double? Tax
        {
            get => GetProperty(() => Tax);
            set
            {
                if (SetProperty(() => Tax, value) && !_isUpdating)
                {
                    if (TaxValue > 0 && value.HasValue)
                    {
                        _isUpdating = true;
                        // recompute base price from new tax
                        PriceWithoutTax = value.Value / TaxValue;
                        // recompute total
                        PriceWithTax = PriceWithoutTax + value;
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
                if (SetProperty(() => PriceWithTax, value) && !_isUpdating)
                {
                    _isUpdating = true;
                    // recompute tax from new total
                    Tax = (value * TaxValue) / (1 + TaxValue);
                    // recompute base
                    PriceWithoutTax = value - Tax;
                    _isUpdating = false;
                }
            }
        }

        /// <summary>
        /// Full recompute when you have a valid PriceWithoutTax & TaxValue.
        /// </summary>
        private void RecalculateFromPrice()
        {
            if (PriceWithoutTax.HasValue)
            {
                _isUpdating = true;
                Tax = PriceWithoutTax * TaxValue;
                PriceWithTax = PriceWithoutTax + Tax;
                _isUpdating = false;
            }
        }
    }
}
