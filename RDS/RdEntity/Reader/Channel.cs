using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RdEntity.Reader
{
    public class Channel
    {
        [XmlAttribute(AttributeName = "strTime")]
        public string strTime { get; set; }
        [XmlAttribute(AttributeName = "strRaw")]
        public string strRaw { get; set; }
        [XmlAttribute(AttributeName = "strValue")]
        public string strValue { get; set; }
    }
}
