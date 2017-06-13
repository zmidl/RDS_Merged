using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RdEntity.Reader
{
    public class Cell
    {
         [XmlAttribute(AttributeName = "nPos")]
        public int nPos { get; set; }
         [XmlAttribute(AttributeName = "strItemName")]
        public string strItemName { get; set; }
         [XmlAttribute(AttributeName = "dtEnzymeTime")]
        public DateTime dtEnzymeTime { get; set; }
         [XmlAttribute(AttributeName = "Result")]
        public Result Result { get; set; }

        public Cell()
        {
            Result = new Result();
        }
    }
}
