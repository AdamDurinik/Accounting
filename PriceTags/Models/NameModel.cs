using DevExpress.Mvvm;

namespace PriceTags.Models
{
    public class NameModel : BindableBase
    {
        public NameModel(string name)
        {
            Name = name;
        }

        public string Name
        {
            get => GetProperty(() => Name);
            set => SetProperty(() => Name, value);
        }
        public NameModel()
        {
            {
            }
        }
    }
}
