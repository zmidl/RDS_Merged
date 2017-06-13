using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sias.PipArm;

namespace RdCore
{
    public class RdAspTemp : SAspirateTemplate
    {
        public override void MoveToPipettingXY(STipMap Tips, double XMin, double XMax, double[] YMin, double[] YMax)
        {
            double x = 0;
            double y = 0;
            double safeZ= ProcessWorkbench.Workbench.PipMethods.Arm.YZDevices[0].ZWorktablePos(0);
            
            PipettingMethods.BlockMoveZ(Tips,safeZ,0,0);

            if ((PipettingMethods.Arm.XWorktablePosition < x) ||
                (PipettingMethods.Arm.YZDevices[0].YWorktablePosition < y))
            {
                //special move
                //PipettingMethods.MoveXY();
                //PipettingMethods.MoveXY();
                //...
                //PipettingMethods.MoveXY();
            }
            base.MoveToPipettingXY(Tips, XMin, XMax, YMin, YMax);
        }

        public RdAspTemp(SPipettingPar Air1, SPipettingPar Vol, SPipettingPar Air2, SPipettingPar Spit, SMixPar MixPar,
            SDetectionPar DetPar) : base(Air1, Vol, Air2, Spit, MixPar, DetPar)
        {
            //
        }

    }
}
