using DevExpress.Mvvm;
using System.Collections.ObjectModel;

namespace Accounting.Models
{
    public class ColumnModel : BindableBase
    {
        public int Tax
        {
            get => GetProperty(() => Tax);
            set
            {
                SetProperty(() => Tax, value);
                foreach(var item in Items)
                {
                    item.UpdateWithTax(value);
                }
            }
        }

        public ObservableCollection<ItemModel> Items { get; set; } = new ObservableCollection<ItemModel>();
    }
}
