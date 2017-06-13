using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdEntity.WorkFlow
{
    public class Nap
    {
        public long Id { get; set; }

        /// <summary>
        /// 六联排编号（1~21）
        /// </summary>
        public int NapId { get; set; }

        /// <summary>
        /// 当前步骤（1~46）
        /// </summary>
        public int CurrentStep { get; set; }

        /// <summary>
        /// 当前步骤是否完成
        /// 0：完成        1：未完成
        /// </summary>
        public int IsCurrentStepFinished { get; set; }

        /// <summary>
        /// 当前六联排是否存在
        /// 0：不存在       1：存在
        /// </summary>
        public int IsCurrentNapExist { get; set; }
    }
}
