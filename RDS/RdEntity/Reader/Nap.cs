using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RdEntity.Reader
{
    public class Nap
    {
         [XmlAttribute(AttributeName = "nID")]
        public int nID { get; set; }
         [XmlAttribute(AttributeName = "nCurrentPos")]
        public int nCurrentPos { get; set; }
         [XmlAttribute(AttributeName = "Cell")]
        public List<Cell> Cell { get; set; }

        public Nap()
        {
            Cell = new List<Cell>();
        }
    }
}
