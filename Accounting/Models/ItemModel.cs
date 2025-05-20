using System.ComponentModel;

namespace Accounting.Models
{
    public class ItemModel : INotifyPropertyChanged
    {

        public ItemModel()
        {

        }

        public ItemModel(string csvValue)
        {
            var values = csvValue.Split(',');
            Id = int.Parse(values[0]);
            TotalPrice = double.Parse(values[1]);
            for (int i = 2; i < values.Length; i++)
            {
                TaxValues.Add(int.Parse(values[i]));
            }
        }

        private double _totalPrice;

        public int Id { get; set; }

        public double TotalPrice
        {
            get => _totalPrice;
            set
            {
                if (_totalPrice != value)
                {
                    _totalPrice = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalPrice)));
                }
            }
        }

        public List<int> TaxValues { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
