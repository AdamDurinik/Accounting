using System.Xml.Serialization;

namespace Accounting.Entity
{
    [XmlRoot("Column")]
    public class ColumnEntity
    {
        [XmlAttribute("DPHValue")]
        public int Tax { get; set; }

        [XmlElement("Items")]
        public List<ItemEntity> Items { get; set; } = new();
    }
}
