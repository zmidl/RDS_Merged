using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdEntity.WorkFlow
{
    public enum ReagentItem
    {
        UU = 0,
        CT = 1,
        MG = 2
    }
    public class Sample
    {
        public long Id { get; set; }

        /// <summary>
        /// 样本位置（1~80）
        /// </summary>
        public int SamplePos { get; set; }

        /// <summary>
        /// 所做项目1
        /// </summary>
        public ReagentItem Item1 { get; set; }

        /// <summary>
        /// 所做项目2
        /// </summary>
        public ReagentItem Item2 { get; set; }

        /// <summary>
        /// 所做项目3
        /// </summary>
        public ReagentItem Item3 { get; set; }

        /// <summary>
        /// 所做项目4
        /// </summary>
        public ReagentItem Item4 { get; set; }

        /// <summary>
        /// 是否急诊
        /// </summary>
        public int IsEmergency { get; set; }

        /// <summary>
        /// 是否等待取样
        /// 0：样本已取用     1：样本待取用
        /// </summary>
        public int IsWaitingForTake { get; set; }

        /// <summary>
        /// 样本条码
        /// </summary>
        public string Barcode { get; set; }

        public Sample()
        {

        }

    }
}
