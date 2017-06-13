using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RdEntity.Reader
{
    public class SetReaderTempr
    {
         [XmlAttribute(AttributeName = "bMustReturn")]
        public bool bMustReturn { get; set; }
         [XmlAttribute(AttributeName = "nTempr")]
        public int nTempr { get; set; }
    }
}
