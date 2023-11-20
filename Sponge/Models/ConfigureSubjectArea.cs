using System.Linq;

namespace Sponge.Models
{
    public class Dimension
    {       
        public string Key { get; set; }
        public string Value { get; set; }
        public bool IsSelected { get; set; }
        
    }
    public class SaveMaster
    {
        public string Master { get; set; }
        public string FieldName { get; set; }
        public string DisplayName { get; set; }

    }
    public class SaveDataCollection
    {
        public string DisplayName { get; set; }
        public string FieldName { get; set; }
        public string IsLookUp { get; set; }
        public string LookUpType { get; set; }
        public string DataType { get; set; }
        public string UOM { get; set; }
    }
}