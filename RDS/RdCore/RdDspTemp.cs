using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sias.PipArm;

namespace RdCore
{
    public class RdDspTemp : SDispenseTemplate
    {
        public override void MoveToPipettingXY(STipMap Tips, double XMin, double XMax, double[] YMin, double[] YMax)
        {
            double x = 0;
            double y = 0;
            double safeZ = ProcessWorkbench.Workbench.PipMethods.Arm.YZDevices[0].ZWorktablePos(0);

            PipettingMethods.BlockMoveZ(Tips, safeZ, 0, 0);

            if ((PipettingMethods.Arm.XWorktablePosition < x) ||
                (PipettingMethods.Arm.YZDevices[0].YWorktablePosition < y))
            {
                //special move
                //PipettingMethod.MoveXY();
                //PipettingMethod.MoveXY();
                //...
                //PipettingMethod.MoveXY();
            }
            base.MoveToPipettingXY(Tips, XMin, XMax, YMin, YMax);
        }

        public RdDspTemp(SPipettingPar DspVol, SMixPar MixPar, SDetectionPar DetPar, SPipettingPar Air2) : base(DspVol, MixPar, DetPar, Air2)
        { }
    }
}
