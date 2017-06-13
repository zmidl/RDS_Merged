using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RdEntity.Reader
{
    public class FluroData
    {
         [XmlAttribute(AttributeName = "bMustReturn")]
        public bool bMustReturn { get; set; }
         [XmlAttribute(AttributeName = "Nap")]
        public List<Nap> Nap { get; set; }

        public FluroData()
        {
            Nap = new List<Nap>();
        }
    }
}
