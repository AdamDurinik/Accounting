using System.Xml.Serialization;

namespace Accounting.Entity
{
    [XmlRoot("File")]
    public class RootEntity
    {
        [XmlElement("Columns")]
        public List<ColumnEntity> Columns { get; set; } = new();
    }
}
