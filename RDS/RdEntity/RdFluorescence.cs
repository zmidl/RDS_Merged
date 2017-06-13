using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdEntity
{
    public class RdFluorescence
    {
        public long Id { get; set; }

        /// <summary>
        /// 测试时间
        /// </summary>
        public DateTime TestTime { get; set; }

        /// <summary>
        /// 原始值
        /// </summary>
        public double RawData { get; set; }

        /// <summary>
        /// 平滑值
        /// </summary>
        public double SmoothData { get; set; }
    }
}
