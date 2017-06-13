using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RdCore;
using RdEntity.WorkFlow;
using Sias.Core.Interfaces;

namespace RdEntity
{
    public class RdData
    {
        /// <summary>
        /// 所做项目1
        /// </summary>
        public int Item1Id { get; set; }
        /// <summary>
        /// 所做项目2
        /// </summary>
        public int Item2Id { get; set; }
        /// <summary>
        /// 所做项目3
        /// </summary>
        public int Item3Id { get; set; }
        /// <summary>
        /// 所做项目4
        /// </summary>
        public int Item4Id { get; set; }

		private int _usedNapCount;
		/// <summary>
		/// 使用的六联排总数
		/// </summary>
		/// <returns></returns>
		public int UsedNapCount
		{
			get {
				int cellCount = GetCellCount(_samples);
				_usedNapCount = cellCount / 6;
				if (cellCount % 6 > 0)
				{
					_usedNapCount++;
				}
				return _usedNapCount; }
			private set
			{
				
			}
		}

		/// <summary>
		/// 获取样本孔位总数
		/// </summary>
		/// <param name="samples">样本集合</param>
		/// <returns></returns>
		private int GetCellCount(ICollection<Sample> samples)
		{
			int count = 0;
			int isDoItem1 = 0;
			int isDoItem2 = 0;
			int isDoItem3 = 0;
			int isDoItem4 = 0;
			foreach (Sample sample in samples)
			{
				if (sample.SmpPos <= 80)
				{
					count += sample.MaxItemCount;//每个样本做几个项目
					if (sample.IsDoItem1 == 1)
					{
						isDoItem1 = 1;
					}
					if (sample.IsDoItem2 == 1)
					{
						isDoItem2 = 1;
					}
					if (sample.IsDoItem3 == 1)
					{
						isDoItem3 = 1;
					}
					if (sample.IsDoItem4 == 1)
					{
						isDoItem4 = 1;
					}
				}
			}
			count = count + (isDoItem1 + isDoItem2 + isDoItem3 + isDoItem4) * 2;//样本所用孔数+阴阳对照所用孔数；
			return count;
		}

		#region 项目1试剂液量

		private ICavity _item1Amp;
        /// <summary>
        /// 项目1扩增检测液当前液量
        /// </summary>
        public double Item1AmpVolume
        {
            get { return _item1Amp.Liquid.Volume; }
            set
            {
                _item1Amp = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_AMPBottole1:RD_AMPBottole_Cavity1");
            }
        }

        private ICavity _item1Negative;
        /// <summary>
        /// 项目1阴性对照当前液量
        /// </summary>
        public double Item1NegativeVolume
        {
            get { return _item1Negative.Liquid.Volume; }
            set
            {
                _item1Negative = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_PNBottole1:RD_PNBottole_Cavity1");
            }
        }

        private ICavity _item1Is;
        /// <summary>
        /// 项目1内标当前液量
        /// </summary>
        public double Item1IsVolume
        {
            get { return _item1Is.Liquid.Volume; }
            set
            {
                _item1Is = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_ISBottole1:RD_Bottole_Cavity1");
            }
        }

        private ICavity _item1Positive;
        /// <summary>
        /// 项目1阳性对照当前液量
        /// </summary>
        public double Item1PositiveVolume
        {
            get { return _item1Positive.Liquid.Volume; }
            set
            {
                _item1Positive = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_PNBottole5:RD_PNBottole_Cavity1");
            }
        }

        private ICavity _item1Ez;
        /// <summary>
        /// 项目1酶当前液量
        /// </summary>
        public double Item1EzVolume
        {
            get { return _item1Ez.Liquid.Volume; }
            set
            {
                _item1Ez = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_EzBottole1:RD_Bottole_Cavity1");
            }
        }

        #endregion 项目1试剂液量

        #region 项目2试剂液量

        private ICavity _item2Amp;
        /// <summary>
        /// 项目1扩增检测液当前液量
        /// </summary>
        public double Item2AmpVolume
        {
            get { return _item2Amp.Liquid.Volume; }
            set
            {
                _item2Amp = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_AMPBottole2:RD_AMPBottole_Cavity1");
            }
        }

        private ICavity _item2Negative;
        /// <summary>
        /// 项目2阴性对照当前液量
        /// </summary>
        public double Item2NegativeVolume
        {
            get { return _item2Negative.Liquid.Volume; }
            set
            {
                _item2Negative = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_PNBottole2:RD_PNBottole_Cavity1");
            }
        }

        private ICavity _item2Is;
        /// <summary>
        /// 项目2内标当前液量
        /// </summary>
        public double Item2IsVolume
        {
            get { return _item2Is.Liquid.Volume; }
            set
            {
                _item2Is = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_ISBottole2:RD_Bottole_Cavity1");
            }
        }

        private ICavity _item2Positive;
        /// <summary>
        /// 项目2阳性对照当前液量
        /// </summary>
        public double Item2PositiveVolume
        {
            get { return _item2Positive.Liquid.Volume; }
            set
            {
                _item2Positive = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_PNBottole6:RD_PNBottole_Cavity1");
            }
        }

        private ICavity _item2Ez;
        /// <summary>
        /// 项目2酶当前液量
        /// </summary>
        public double Item2EzVolume
        {
            get { return _item2Ez.Liquid.Volume; }
            set
            {
                _item2Ez = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_EzBottole2:RD_Bottole_Cavity1");
            }
        }

        #endregion 项目2试剂液量

        #region 项目3试剂液量

        private ICavity _item3Amp;
        /// <summary>
        /// 项目1扩增检测液当前液量
        /// </summary>
        public double Item3AmpVolume
        {
            get { return _item3Amp.Liquid.Volume; }
            set
            {
                _item3Amp = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_AMPBottole3:RD_AMPBottole_Cavity1");
            }
        }

        private ICavity _item3Negative;
        /// <summary>
        /// 项目3阴性对照当前液量
        /// </summary>
        public double Item3NegativeVolume
        {
            get { return _item3Negative.Liquid.Volume; }
            set
            {
                _item3Negative = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_PNBottole3:RD_PNBottole_Cavity1");
            }
        }

        private ICavity _item3Is;
        /// <summary>
        /// 项目3内标当前液量
        /// </summary>
        public double Item3IsVolume
        {
            get { return _item3Is.Liquid.Volume; }
            set
            {
                _item3Is = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_ISBottole3:RD_Bottole_Cavity1");
            }
        }

        private ICavity _item3Positive;
        /// <summary>
        /// 项目3阳性对照当前液量
        /// </summary>
        public double Item3PositiveVolume
        {
            get { return _item3Positive.Liquid.Volume; }
            set
            {
                _item3Positive = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_PNBottole7:RD_PNBottole_Cavity1");
            }
        }

        private ICavity _item3Ez;
        /// <summary>
        /// 项目3酶当前液量
        /// </summary>
        public double Item3EzVolume
        {
            get { return _item3Ez.Liquid.Volume; }
            set
            {
                _item3Ez = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_EzBottole3:RD_Bottole_Cavity1");
            }
        }

        #endregion 项目3试剂液量

        #region 项目4试剂液量

        private ICavity _item4Amp;
        /// <summary>
        /// 项目1扩增检测液当前液量
        /// </summary>
        public double Item4AmpVolume
        {
            get { return _item4Amp.Liquid.Volume; }
            set
            {
                _item4Amp = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_AMPBottole4:RD_AMPBottole_Cavity1");
            }
        }

        private ICavity _item4Negative;
        /// <summary>
        /// 项目4阴性对照当前液量
        /// </summary>
        public double Item4NegativeVolume
        {
            get { return _item4Negative.Liquid.Volume; }
            set
            {
                _item4Negative = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_PNBottole4:RD_PNBottole_Cavity1");
            }
        }

        private ICavity _item4Is;
        /// <summary>
        /// 项目4内标当前液量
        /// </summary>
        public double Item4IsVolume
        {
            get { return _item4Is.Liquid.Volume; }
            set
            {
                _item4Is = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_ISBottole4:RD_Bottole_Cavity1");
            }
        }

        private ICavity _item4Positive;
        /// <summary>
        /// 项目4阳性对照当前液量
        /// </summary>
        public double Item4PositiveVolume
        {
            get { return _item4Positive.Liquid.Volume; }
            set
            {
                _item4Positive = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_PNBottole8:RD_PNBottole_Cavity1");
            }
        }

        private ICavity _item4Ez;
        /// <summary>
        /// 项目4酶当前液量
        /// </summary>
        public double Item4EzVolume
        {
            get { return _item4Ez.Liquid.Volume; }
            set
            {
                _item4Ez = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_EzBottole4:RD_Bottole_Cavity1");
            }
        }

        #endregion 项目4试剂液量

        #region 公共试剂液量

        private ICavity _mb1;
        private ICavity _mb2;

        /// <summary>
        /// 磁珠1液量
        /// </summary>
        public double Mb1Volume
        {
            get { return _mb1.Liquid.Volume; }
            set { _mb1 = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_MBBottole1:RD_MBBottole_Cavity1"); }
        }

        /// <summary>
        /// 磁珠2液量
        /// </summary>
        public double Mb2Volume
        {
            get { return _mb2.Liquid.Volume; }
            set { _mb2 = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_MBBottole2:RD_MBBottole_Cavity1"); }
        }

        private ICavity _box1;
        private ICavity _box2;
        private ICavity _box3;
        private ICavity _box4;
        private ICavity _box5;
        private ICavity _box6;
        private ICavity _box7;
        private ICavity _box8;

        /// <summary>
        /// 尿保液量
        /// </summary>
        public double BufferVolume
        {
            get { return _box1.Liquid.Volume; }
            set { _box1 = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_ReagentBox1:RD_ReagentBox_Cavity1"); }
        }

        /// <summary>
        /// 生理盐水液量
        /// </summary>
        public double SalinVolume
        {
            get { return _box2.Liquid.Volume; }
            set { _box2 = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_ReagentBox2:RD_ReagentBox_Cavity1"); }
        }

        /// <summary>
        /// 矿物油液量
        /// </summary>
        public double OilVolume
        {
            get { return _box3.Liquid.Volume; }
            set { _box3 = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_ReagentBox3:RD_ReagentBox_Cavity1"); }
        }

        /// <summary>
        /// 洗液1液量
        /// </summary>
        public double Washing1Volume
        {
            get { return _box4.Liquid.Volume; }
            set { _box4 = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_ReagentBox4:RD_ReagentBox_Cavity1"); }
        }

        /// <summary>
        /// 洗液2液量
        /// </summary>
        public double Washing2Volume
        {
            get { return _box8.Liquid.Volume; }
            set { _box8 = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_ReagentBox8:RD_ReagentBox_Cavity1"); }
        }

        private ICavity _paraffin;

        /// <summary>
        /// 石蜡液量
        /// </summary>
        public double ParaffinVolume
        {
            get { return _paraffin.Liquid.Volume; }
            set
            {
                _paraffin = ProcessWorkbench.Workbench.Layout.GetCavityByName("RD_ParaffinBox1:RD_ParaffinBox_Cavity1");
            }
        }

        #endregion 公共试剂液量

        private Dictionary<int, int> _napPos;
        /// <summary>
        /// 六联排所在位置
        /// key:六联排编号       value:六联排所在位置（1~38）
        /// </summary>
        public Dictionary<int, int> NapPos
        {
            get { return _napPos; }
            set
            {
                for (int i = 1; i <= 21; i++)
                {
                    string napName = string.Format("RD_Cup{0}", i);
                    int pos = 0;
                    IItem nap=ProcessWorkbench.Workbench.Layout.GetItemByName(napName);
                    if (nap != null)
                    {
                        ISlot slot = nap.CurrentSlot;
                        switch (slot.Name)
                        {
                            case "RD_CupRack1:RD_CupRack_CupSlot1":
                                pos = 1;
                                break;
                            case "RD_CupRack1:RD_CupRack_CupSlot2":
                                pos = 2;
                                break;
                            case "RD_CupRack1:RD_CupRack_CupSlot3":
                                pos = 3;
                                break;
                            case "RD_CupRack1:RD_CupRack_CupSlot4":
                                pos = 4;
                                break;
                            case "RD_CupRack1:RD_CupRack_CupSlot5":
                                pos = 5;
                                break;
                            case "RD_CupRack1:RD_CupRack_CupSlot6":
                                pos = 6;
                                break;
                            case "RD_CupRack1:RD_CupRack_CupSlot7":
                                pos = 7;
                                break;
                            case "RD_CupRack2:RD_CupRack_CupSlot1":
                                pos = 8;
                                break;
                            case "RD_CupRack2:RD_CupRack_CupSlot2":
                                pos = 9;
                                break;
                            case "RD_CupRack2:RD_CupRack_CupSlot3":
                                pos = 10;
                                break;
                            case "RD_CupRack2:RD_CupRack_CupSlot4":
                                pos = 11;
                                break;
                            case "RD_CupRack2:RD_CupRack_CupSlot5":
                                pos = 12;
                                break;
                            case "RD_CupRack2:RD_CupRack_CupSlot6":
                                pos = 13;
                                break;
                            case "RD_CupRack2:RD_CupRack_CupSlot7":
                                pos = 14;
                                break;
                            case "RD_CupRack3:RD_CupRack_CupSlot1":
                                pos = 15;
                                break;
                            case "RD_CupRack3:RD_CupRack_CupSlot2":
                                pos = 16;
                                break;
                            case "RD_CupRack3:RD_CupRack_CupSlot3":
                                pos = 17;
                                break;
                            case "RD_CupRack3:RD_CupRack_CupSlot4":
                                pos = 18;
                                break;
                            case "RD_CupRack3:RD_CupRack_CupSlot5":
                                pos = 19;
                                break;
                            case "RD_CupRack3:RD_CupRack_CupSlot6":
                                pos = 20;
                                break;
                            case "RD_CupRack3:RD_CupRack_CupSlot7":
                                pos = 21;
                                break;
                            case "RD_Heating1:RD_Heating_CupSlot1":
                                pos = 22;
                                break;
                            case "RD_Heating1:RD_Heating_CupSlot2":
                                pos = 23;
                                break;
                            case "RD_Heating1:RD_Heating_CupSlot3":
                                pos = 24;
                                break;
                            case "RD_Heating1:RD_Heating_CupSlot4":
                                pos = 25;
                                break;
                            case "RD_ShakerRack1:RD_ShakerRack_CupSlot1":
                                pos = 26;
                                break;
                            case "RD_ShakerRack1:RD_ShakerRack_CupSlot2":
                                pos = 27;
                                break;
                            case "RD_ShakerRack1:RD_ShakerRack_CupSlot3":
                                pos = 28;
                                break;
                            case "RD_Mag1:RD_Mag_CupSlot1":
                                pos = 29;
                                break;
                            case "RD_Mag1:RD_Mag_CupSlot2":
                                pos = 30;
                                break;
                            case "RD_Mag1:RD_Mag_CupSlot3":
                                pos = 31;
                                break;
                            case "RD_Mag1:RD_Mag_CupSlot4":
                                pos = 32;
                                break;
                            case "RD_Reader1:RD_Reader_CupSlot1":
                                pos = 33;
                                break;
                            case "RD_Reader1:RD_Reader_CupSlot2":
                                pos = 34;
                                break;
                            case "RD_Reader1:RD_Reader_CupSlot3":
                                pos = 35;
                                break;
                            case "RD_Reader1:RD_Reader_CupSlot4":
                                pos = 36;
                                break;
                            case "RD_Reader1:RD_Reader_CupSlot5":
                                pos = 37;
                                break;
                            case "RD_Reader1:RD_Reader_CoolDownSlot1":
                                pos = 38;
                                break;
                        }
                    }
                    _napPos.Add(i,pos);
                }
            }
        }

		private ICollection<Sample> _samples;
		/// <summary>
		/// 样本集合
		/// </summary>
		public virtual ICollection<Sample> Samples
		{
			get { return _samples; }
			set { _samples = value; }
		}

        /// <summary>
        /// 六联排集合
        /// </summary>
        public virtual ICollection<Nap> Naps { get; set; }

        /// <summary>
        /// 下一步流程集合（对每个六联排）
        /// </summary>
        public virtual  ICollection<NextStep> NextSteps { get; set; }

        public RdData()
        {
            Samples=new List<Sample>();
            Naps=new List<Nap>();
            NextSteps=new List<NextStep>();
            for (int i = 1; i <= 88; i++)
            {
                Sample sample=new Sample(i);
				sample.IsWaitingForTake = 1;
                Samples.Add(sample);
            }
			for(int i=1;i<=21;i++)
			{
				Nap nap = new Nap();
				nap.NapId = i;
				nap.CurrentPos = i;
				nap.CurrentStep = 1;
				nap.IsCurrentNapExist = 0;
			    nap.IsCurrentNapUsed = 0;
				nap.IsCurrentStepFinished = 0;
				Naps.Add(nap);
			}
			for(int i=0;i<21;i++)
			{
				NextStep nextStep = new NextStep();
				nextStep.NapId = i;
				nextStep.IsStateChanged =1;
				nextStep.Step = i;
				
				NextSteps.Add(nextStep);
			}
        }

        /// <summary>
        /// 转成样本数组
        /// </summary>
        /// <returns></returns>
        public int[,] ToSampleArray()
        {
            int[,] array = new int[88,7];

            for (int i = 0; i < 88; i++)
            {
                var sample = Samples.FirstOrDefault(s => s.SmpPos == i + 1);
                if (sample == null)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        array[i, j] = 0;
                    }
                }
                else
                {
                    for (int j = 0; j < 7; j++)
                    {
                        switch (j)
                        {
                            case 0:
                                array[i,j] = sample.SmpPos;
                                break;
                            case 1:
                                array[i,j] = sample.IsDoItem1;
                                break;
                            case 2:
                                array[i,j] = sample.IsDoItem2;
                                break;
                            case 3:
                                array[i,j] = sample.IsDoItem3;
                                break;
                            case 4:
                                array[i,j] = sample.IsDoItem4;
                                break;
                            case 5:
                                array[i,j] = (int)sample.IsEmergency;
                                break;
                            case 6:
                                array[i,j] = sample.IsWaitingForTake;
                                break;
                        }
                    }
                }
            }
            for (int m = 0; m < 8; m++)
            {
                
            }
            return array;
        }

        /// <summary>
        /// 转成六联排数组
        /// </summary>
        /// <returns></returns>
        public int[,] ToNapArray()
        {
            int[,] array = new int[21,4];

            for (int i = 0; i < 21; i++)
            {
                var nap = Naps.FirstOrDefault(n => n.NapId == i + 1);
                if (nap == null)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        array[i, j] = 0;
                    }
                }
                else
                {
                    for (int j = 0; j < 4; j++)
                    {
                        switch (j)
                        {
                            case 0:
                                array[i, j] = nap.NapId;
                                break;
                            case 1:
                                array[i, j] = nap.CurrentStep;
                                break;
                            case 2:
                                array[i, j] = nap.IsCurrentStepFinished;
                                break;
                            case 3:
                                array[i, j] = nap.IsCurrentNapUsed;
                                break;
                        }
                    }
                }
            }
            return array;
        }


        /// <summary>
        /// 数组转成对象
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public ICollection<NextStep> FromArray(Int16[,] array)
        {
            ICollection<NextStep> nextSteps = new List<NextStep>();
            for (int i = 0; i < 21; i++)
            {
                NextStep nextStep = new NextStep();
                for (int j = 0; j < 9; j++)
                {
                    switch (j)
                    {
                        case 0:
                            nextStep.NapId = array[i, j];
                            break;
                        case 1:
                            nextStep.Step = array[i, j];
                            break;
                        case 2:
                            nextStep.Tip1Asp = array[i, j];
                            break;
                        case 3:
                            nextStep.Tip2Asp = array[i, j];
                            break;
                        case 4:
                            nextStep.Tip3Asp = array[i, j];
                            break;
                        case 5:
                            nextStep.Tip1Dsp = array[i, j];
                            break;
                        case 6:
                            nextStep.Tip2Dsp = array[i, j];
                            break;
                        case 7:
                            nextStep.Tip3Dsp = array[i, j];
                            break;
                        case 8:
                            nextStep.IsStateChanged = array[i, j];
                            break;
                    }
                }
                nextSteps.Add(nextStep);
            }
            NextSteps = nextSteps;
            return nextSteps;
        }
    }
}
