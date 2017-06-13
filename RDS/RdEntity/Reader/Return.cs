using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RdEntity.Reader
{
    public class Return
    {
        [XmlAttribute(AttributeName = "strName")]
        public string strName { get; set; }
        [XmlAttribute(AttributeName = "nResult")]
        public int nResult { get; set; }
    }
}
