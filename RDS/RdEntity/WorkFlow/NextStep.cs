using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdEntity.WorkFlow
{
    public class NextStep
    {
        public long Id { get; set; }

        /// <summary>
        /// 六联排编号（1~21）
        /// </summary>
        public int NapId { get; set; }

        /// <summary>
        /// 步骤（1~46）
        /// </summary>
        public int Step { get; set; }

        /*
        *先判断步骤，如果是加样，则用1 ~80表示样本位置；
        *如果是加项目试剂，则用1 ~4表示对应项目；
        *如果是加公用使用，则可为0；
        *如果是抽废液，则用1 ~6表示当前六联排上的第几个孔位。
        */
        /// <summary>
        /// Tip1去哪吸
        /// </summary>
        public int Tip1Asp { get; set; }

        /// <summary>
        /// Tip2去哪吸
        /// </summary>
        public int Tip2Asp { get; set; }

        /// <summary>
        /// Tip2去哪吸
        /// </summary>
        public int Tip3Asp { get; set; }

        /*
         * 先判断步骤，如果是加样/加试剂/加公用试剂，则用1~6表示加到当前六联排的第几个孔位；
         * 如果是抽废液，则可为0；
         */
        /// <summary>
        /// Tip1加到哪
        /// </summary>
        public int Tip1Dsp { get; set; }

        /// <summary>
        /// Tip2加到哪
        /// </summary>
        public int Tip2Dsp { get; set; }

        /// <summary>
        /// Tip3加到哪
        /// </summary>
        public int Tip3Dsp { get; set; }

        /// <summary>
        /// 状态是否改变
        /// 0：未改变       1：改变
        /// </summary>
        public int IsStateChanged { get; set; }
    }
}
