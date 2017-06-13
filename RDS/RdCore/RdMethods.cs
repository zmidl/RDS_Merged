using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sias.Core.Interfaces;
using Sias.PipArm;

namespace RdCore
{
    public class RdMethods
    {
        private ProcessWorkbench workbench;
        /// <summary>
        /// 读条码
        /// </summary>
        /// <param name="slotIndex">第几条样本</param>
        /// <returns></returns>
        public Dictionary<int, string> ReadBarcode(int slotIndex)
        {
            Dictionary<int,string> dicBarcodes=new Dictionary<int, string>();
            return dicBarcodes;
        }

        /// <summary>
        /// 搬运六联排
        /// </summary>
        /// <param name="sourceItem">六联排</param>
        /// <param name="desSlot">目标位置</param>
        /// <returns></returns>
        public bool MoveItem(IItem sourceItem, ISlot desSlot)
        {
            bool flag = false;
            workbench.MoveItem(sourceItem,desSlot);
            return flag;
        }

        /// <summary>
        /// 冲针
        /// </summary>
        /// <param name="volume">体积（默认400ul）</param>
        public void Flush(double volume = 400)
        {
            workbench.MoveToWashStation();
            workbench.FlushADP(volume);
        }

        /// <summary>
        /// 取Tip头
        /// </summary>
        /// <param name="usedTips">哪几个通道去取</param>
        /// <param name="tipType">Tip头类型（300/1000）</param>
        /// <returns></returns>
        public bool GetTips(STipMap usedTips, ITipType tipType)
        {
            bool flag = false;
            return flag;
        }

        /// <summary>
        /// 吸液
        /// </summary>
        /// <param name="usedTips">哪几个通道去吸液</param>
        /// <param name="cavities">吸液孔</param>
        /// <returns></returns>
        public bool Aspirate(STipMap usedTips,ICollection<ICavity> aspCavities)
        {
            bool flag = false;
            return flag;
        }

        /// <summary>
        /// 打液
        /// </summary>
        /// <param name="usedTips">哪几个通道去打液</param>
        /// <param name="cavities">打液孔</param>
        /// <returns></returns>
        public bool Dispense(STipMap usedTips, ICollection<ICavity> dspCavities)
        {
            bool flag = false;
            return flag;
        }

        /// <summary>
        /// 打Tip头
        /// </summary>
        /// <param name="usedTips">哪几个通道去打Tip头</param>
        /// <returns></returns>
        public bool DropTips(STipMap usedTips)
        {
            bool flag = false;
            return flag;
        }


    }
}
