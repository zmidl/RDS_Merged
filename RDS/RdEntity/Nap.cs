using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RdEntity
{
    public enum WorkFlowStep
    {
        Init = 1,
        AddNegative=2,
        AddPositive=3,
        AddSalin=4,
        AddMb=5,
        AddIs1=6,
        AddIs2=7,
        AddSample1=8,
        AddSample2=9,
        FirstMoveToShaker=10,
        FirstShake30=11,
        FirstMoveToHeating=12,
        FirstHeating600=13,
        FirstMoveToCool=14,
        FirstCool600=15,
        FirstMoveToMag=16,
        FirstMag120=17,
        FirstWaste1=18,
        FirstWaste2=19,
        SecondMoveToCool=20,
        FirstWashing=21,
        SecondMoveToShaker=22,
        SecondShake30=23,
        SecondMoveToMag=24,
        SecondMag120=25,
        SecondWaste1=26,
        SecondWaste2=27,
        ThirdMoveToCool=28,
        SecondWashing=29,
        AddOil=30,
        ThirdMoveToShaker=31,
        ThirdShake30=32,
        ThirdMoveToMag=33,
        ThirdMag120=34,
        ThirdWaste1=35,
        ThirdWaste2=36,
        FourthMoveToCool=37,
        AddAmp1=38,
        AddAmp2=39,
        FourthMoveToShaker=40,
        FourthShake60=41,
        SecondMoveToHeating=42,
        AddParaffin=43,
        SecondHeating600=44,
        MoveToReader=45,
        ReaderWait300=46,
        AddEz1=47,
        AddEz2=48,
        Read2400=49,
        MoveToCoolDown=50,
        CoolDownWait300=51,
        MoveToDrop=52
    }

    public class Nap
    {
        public long Id { get; set; }

        /// <summary>
        /// 六联排编号（1~21）
        /// </summary>
        public int NapId { get; set; }

        /// <summary>
        /// 当前位置
        /// </summary>
        public int CurrentPos { get; set; }

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
        /// 当前六联排是否使用
        /// 0：不使用       1：使用
        /// </summary>
        public int IsCurrentNapUsed { get; set; }

        /// <summary>
        /// 当前六联排是否存在
        /// 0：不存在       1：存在
        /// </summary>
        public int IsCurrentNapExist { get; set; }

        /// <summary>
        /// 孔集合
        /// </summary>
        public ObservableCollection<Cell> Cells { get; set; }

        public Nap()
        {
            Cells=new ObservableCollection<Cell>();
            for (var i = 1; i <= 6; i++)
            {
                Cell cell = new Cell();
                cell.Id = i;
                Cells.Add(cell);
            }
            CurrentStep = 0;
        }

        //private bool _isEmergency;

        ///// <summary>
        ///// 是否急诊
        ///// </summary>
        //public bool IsEmergency
        //{
        //    get
        //    {
        //        return _isEmergency;
        //    }
        //    set
        //    {
        //        foreach (var cell in Cells)
        //        {
        //            if (cell.IsEmergency)
        //            {
        //                _isEmergency = true;
        //                break;
        //            }
        //        }
        //    }
        //}
    }
}
