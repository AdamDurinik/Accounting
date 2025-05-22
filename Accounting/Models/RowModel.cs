using Accounting.Entity;

namespace Accounting.Models
{
    public class RowModel
    {
        public RowModel(List<ItemEntity> items, double tax)
        {
            foreach(var item in items)
            {
                var itemModel = new ItemModel(item, tax);
                Items.Add(itemModel);
            }
        }

        public List<ItemModel> Items { get; set; } = new List<ItemModel>();
    }
}
