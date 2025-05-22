using System.Xml.Serialization;

namespace Accounting.Entity
{
    [XmlRoot("Item")]
    public class ItemEntity
    {
        [XmlAttribute("PriceWithoutDPH")]
        public double PriceWithoutTax { get; set; }
    }
}
