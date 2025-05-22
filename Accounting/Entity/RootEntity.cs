using System.Xml.Serialization;

namespace Accounting.Entity
{
    [XmlRoot("File")]
    public class RootEntity
    {
        [XmlAttribute("FileName")]
        public string FileName { get; set; } = string.Empty;

        [XmlElement("Columns")]
        public List<ColumnEntity> Columns { get; set; } = new();
    }
}
