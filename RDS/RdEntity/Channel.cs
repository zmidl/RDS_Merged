using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdEntity
{
    public class Channel
    {
        public long Id { get; set; }

        /// <summary>
        /// 通道号
        /// </summary>
        public int ChannelId { get; set; }

        /// <summary>
        /// 阴阳性
        /// </summary>
        public bool IsPositive { get; set; }

        /// <summary>
        /// 浓度值
        /// </summary>
        public double Concentration { get; set; }

        /// <summary>
        /// dt值
        /// </summary>
        public DateTime Dt { get; set; }

        /// <summary>
        /// 起跳时间
        /// </summary>
        public DateTime TakeOffTime { get; set; }

        /// <summary>
        /// 荧光值
        /// </summary>
        public List<RdFluorescence> Fluorescences { get; set; }
    }
}
