using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RdCore.Enum
{
    public enum RdAction
    {
        Moving=0,//正在搬运
        MoveItem=1,//搬运完成
        Aspirate=2,//吸液
        Dispense=3,//加液
        Waste=4,//抽废液
        GetTip=5//取Tip头
    }
}
