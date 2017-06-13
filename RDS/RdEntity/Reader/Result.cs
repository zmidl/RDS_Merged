using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RdEntity.Reader
{
    public enum ResultEnum
    {
        Negative = 0,
        Positive = 1,
        WeaklyPositive = 2,
        ExWeaklyPositive = 3,
        GrayArea = 4
    }

    public class Result
    {
         [XmlAttribute(AttributeName = "nCycleCount")]
        public int nCycleCount { get; set; }
         [XmlAttribute(AttributeName = "dCt")]
        public double dCt { get; set; }
         [XmlAttribute(AttributeName = "dConc")]
        public double dConc { get; set; }
         [XmlAttribute(AttributeName = "nResult")]
        public ResultEnum nResult { get; set; }
         [XmlAttribute(AttributeName = "Channel1")]
        public Channel Channel1 { get; set; }
         [XmlAttribute(AttributeName = "Channel2")]
        public Channel Channel2 { get; set; }

        public Result()
        {
            Channel1 = new Channel();
            Channel2 = new Channel();
        }
    }
}
