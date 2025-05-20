using System.ComponentModel;

namespace Accounting.Models
{
    public class ColumnDefinition : INotifyPropertyChanged
    {
        public string Name { get; set; } 
        public int Percentage { get; set; }
 

        public event PropertyChangedEventHandler PropertyChanged;
    }

}
