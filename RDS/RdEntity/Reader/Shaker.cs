using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RdEntity.Reader
{
    public class Shaker
    {
        [XmlAttribute(AttributeName = "bMustReturn")]
        public bool bMustReturn { get; set; }
         [XmlAttribute(AttributeName = "nCmd")]
        public int nCmd { get; set; }
         [XmlAttribute(AttributeName = "nSpeed")]
        public int nSpeed { get; set; }
         [XmlAttribute(AttributeName = "nDealy")]
        public int nDealy { get; set; }
         [XmlAttribute(AttributeName = "nDirection")]
        public int nDirection { get; set; }
    }
}
