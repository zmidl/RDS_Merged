using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RDS.Apps;
using RDS.Views;
using Sias.ReSaTrax;
using RdCore;
using RdEntity.WorkFlow;
using Sias.Core.Interfaces;
using Sias.PipArm;
using RDS.ViewModels.Common;
using RdEntity;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using Rendu.ShareDll;
using RDS.SDK;

namespace RDS
{
	public class Sdk
	{
		public ProcessWorkbench workbench = ProcessWorkbench.Workbench;

		public event EventHandler<SdkMessage.SdkMessageArgs> EventHandler;

		public virtual void OnMyMessageBoxShow(SdkMessage.SdkMessageArgs args)
		{
			this.EventHandler?.Invoke(this, args);
		}

		public Window Window { get; set; }

        /// <summary>
        /// 初始化，仪器复位
        /// </summary>
        public void Init()
        {
            workbench.Init();
            workbench.LoadTipStates("Rendu.TipStates.xml");
        }

        public Sdk()
        {
            workbench.ShakeFinishedEvent += ShakeFinished;
            workbench.WaitFinishedEvent += WaitFinished;
        }

		/// <summary>
		/// 填充指定Tip载架
		/// </summary>
		/// <param name="tipRackId">载架编号0~8；0=300ulTip；1~8=1000ulTip</param>
		public void FillTips(int tipRackIndex)
		{
			workbench.FillTips(tipRackIndex + 1);
		}

		/// <summary>
		/// 读条码
		/// </summary>
		/// <param name="slotIndex">第几条</param>
		/// <returns></returns>
		public Dictionary<int, string> ReadBarcode(int smpRackIndex)
		{
			int column = smpRackIndex + 1; //第几条
			int slotIndex = smpRackIndex + 2; //第一条样本架对应索引为2；

			SReSaTraxDevice ReSaTrax = workbench.Robot.GetDevice("Resatrax1") as SReSaTraxDevice;
			if (ReSaTrax is SReSaTraxDevice)
			{
				for (int i = 0; i < ReSaTrax.MaxSlot; i++)
				{
					if (ReSaTrax.GetSlotState(i).IsOccupied)
					{
						ReSaTrax.SetSlotState(i, SReSaTraxDevice.SSlotState.ExpectedFull);
					}
					else
					{
						ReSaTrax.SetSlotState(i, SReSaTraxDevice.SSlotState.ExpectedEmpty);
					}
				}

				string slotName = string.Format("Resatrax1:RD_ReSaTrax_SampleRackSlot{0}", column);
				ISlot slot = workbench.Layout.GetSlotByName(slotName);
				ReSaTrax.Slot[slotIndex] = slot;
				IItem strip = ReSaTrax.Slot[slotIndex].CurrentItem;

				App.Current.Dispatcher.Invoke(() =>
				{
					if (strip is IItem)
					{
						if (ReSaTrax.GetSlotState(slotIndex).IsOccupied)
						{
							SdkMessage sdkMessage = new SdkMessage(PopupType.OneButton, "RemoveStripWindow", "Please Unload Strip", this.Window);
							sdkMessage.EventHandler += ((s, e) => { this.OnMyMessageBoxShow(sdkMessage.Args); });
							ReSaTrax.RemoveStrip(slotIndex, sdkMessage);
						}
						if (!ReSaTrax.GetSlotState(slotIndex).IsOccupied)
						{
							SdkMessage sdkMessage = new SdkMessage(PopupType.TwoButton, "AddStripWindow", "Please Load Strip", this.Window);
							sdkMessage.EventHandler += ((s, e) => { this.OnMyMessageBoxShow(sdkMessage.Args); });
							ReSaTrax.AddStrip(slotIndex, strip, sdkMessage);
						}
					}
				});
			}

			Dictionary<int, string> dicBarcode = new Dictionary<int, string>();

			for (var i = 0; i < 20; i++)
			{
				string barcode = ReSaTrax.GetRawBarcode(slotIndex, i);
				int pos = (column - 1) * 20 + i + 1; //（第n列-1）*每列20个样本 + 当前列第几个位置；
				dicBarcode.Add(pos, barcode);
			}

			return dicBarcode;
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
			flag = workbench.MoveItem(sourceItem, desSlot);
			return flag;
		}

		/// <summary>
		/// 冲针
		/// </summary>
		/// <param name="volume">体积（默认400ul）</param>
		public void Flush(double volume = 400)
		{
			workbench.FlushADP(volume);
		}

		/// <summary>
		/// 取Tip头
		/// </summary>
		/// <param name="usedTips">哪几个通道去取</param>
		/// <param name="tipV">Tip头类型（300/1000）</param>
		/// <returns></returns>
		public bool GetTips(STipMap usedTips, double tipV = 1000)
		{
			bool flag = false;
			flag = workbench.GetTips(usedTips, tipV);
			return flag;
		}

		/// <summary>
		/// 吸液
		/// </summary>
		/// <param name="usedTips">哪几个通道去吸液</param>
		/// <param name="cavities">吸液孔</param>
		/// <returns></returns>
		public void Aspirate(STipMap usedTips, SCavityCollection aspCavities, double volume)
		{
			workbench.Aspirate(usedTips, aspCavities, volume);
		}

		/// <summary>
		/// 打液
		/// </summary>
		/// <param name="usedTips">哪几个通道去打液</param>
		/// <param name="cavities">打液孔</param>
		/// <returns></returns>
		public void Dispense(STipMap usedTips, SCavityCollection dspCavities, double volume)
		{
			workbench.Dispense(usedTips, dspCavities, volume);
		}

		/// <summary>
		/// 打Tip头
		/// </summary>
		/// <param name="usedTips">哪几个通道去打Tip头</param>
		/// <returns></returns>
		public void DropTips(STipMap usedTips)
		{
			workbench.DropTips(usedTips);
		}

		//public void Transfer(STipMap usedTips, SCavityCollection source, SCavityCollection destination, double volume, double tipV, string category)
		//{
		//    Flush();
		//    GetTips(usedTips, tipV);
		//    Aspirate(usedTips, source,volume);
		//    Dispense(usedTips, destination,volume);
		//    DropTips(usedTips);
		//}

        public void LoopExecute()
        {
            while (GetMinStep(App.GlobalData) < 51)
            {
                if (IsCurrentStepFinished(App.GlobalData.Naps))
                {
                    CalcNext();
                    Execute(App.GlobalData.NextSteps);
                }
            }
        }

        private bool IsCurrentStepFinished(ICollection<Nap> naps)
        {
            bool ret = false;
            foreach (var nap in naps)
            {
                if (nap.IsCurrentNapUsed == 1)
                {
                    if (nap.IsCurrentStepFinished == 0)
                    {
                        ret = true;
                        return ret;
                    }
                }
            }
            return ret;
        }

        private int GetMinStep(RdData rdData)
        {
            //return (from nextStep in rdData.NextSteps where nextStep.IsStateChanged == 1 select nextStep.Step).Concat(new[] { 51 }).Min();

			int minStep = 51;

            foreach (var nap in rdData.Naps)
            {
                if (nap.IsCurrentNapUsed == 1)
                {
                    if (nap.CurrentStep < minStep)
                    {
                        minStep = nap.CurrentStep;
                    }
                }
            }

			return minStep;
		}

		/// <summary>
		/// 执行流程
		/// </summary>
		/// <param name="nextSteps"></param>
		public void Execute(ICollection<NextStep> nextSteps)
		{
			foreach (var nextstep in nextSteps)
			{
				if (nextstep.IsStateChanged == 1)
				{
					SCavityCollection aspCavities = new SCavityCollection();
					SCavityCollection dspCavities = new SCavityCollection();
					ICavity c;
					var nap = App.GlobalData.Naps.FirstOrDefault(n => n.NapId == nextstep.NapId);
					switch (nextstep.Step)
					{
						case 1:
						break;
						case 2:
						dspCavities.Clear();
						if (nextstep.Tip1Asp != 0)
						{
							string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
								nextstep.Tip1Asp);
							c = workbench.Layout.GetCavityByName(cavityName);
							dspCavities.Add(c);
						}
						if (nextstep.Tip2Asp != 0)
						{
							string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
								nextstep.Tip2Asp);
							c = workbench.Layout.GetCavityByName(cavityName);
							dspCavities.Add(c);
						}
						if (nextstep.Tip3Asp != 0)
						{
							string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
								nextstep.Tip3Asp);
							c = workbench.Layout.GetCavityByName(cavityName);
							dspCavities.Add(c);
						}
						if (nextstep.Tip1Dsp != 0)
						{
							string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
								nextstep.Tip1Dsp);
							c = workbench.Layout.GetCavityByName(cavityName);
							dspCavities.Add(c);
						}
						if (nextstep.Tip2Dsp != 0)
						{
							string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
								nextstep.Tip2Dsp);
							c = workbench.Layout.GetCavityByName(cavityName);
							dspCavities.Add(c);
						}
						if (nextstep.Tip3Dsp != 0)
						{
							string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
								nextstep.Tip3Dsp);
							c = workbench.Layout.GetCavityByName(cavityName);
							dspCavities.Add(c);
						}
						workbench.AddMb(dspCavities);
						nap.CurrentStep = 2;
						nap.IsCurrentStepFinished = 0;
						break;
						case 3:
						aspCavities.Clear();
						dspCavities.Clear();
						if (nextstep.Tip1Asp != 0)
						{
							string cavityName = string.Format("RD_ISBottole{0}:RD_Bottole_Cavity1", nextstep.Tip1Asp);
							c = workbench.Layout.GetCavityByName(cavityName);
							aspCavities.Add(c);
						}
						if (nextstep.Tip2Asp != 0)
						{
							string cavityName = string.Format("RD_ISBottole{0}:RD_Bottole_Cavity1", nextstep.Tip2Asp);
							c = workbench.Layout.GetCavityByName(cavityName);
							aspCavities.Add(c);
						}
						if (nextstep.Tip3Asp != 0)
						{
							string cavityName = string.Format("RD_ISBottole{0}:RD_Bottole_Cavity1", nextstep.Tip3Asp);
							c = workbench.Layout.GetCavityByName(cavityName);
							aspCavities.Add(c);
						}
						if (nextstep.Tip1Dsp != 0)
						{
							string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
								nextstep.Tip1Dsp);
							c = workbench.Layout.GetCavityByName(cavityName);
							dspCavities.Add(c);
						}
						if (nextstep.Tip2Dsp != 0)
						{
							string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
								nextstep.Tip2Dsp);
							c = workbench.Layout.GetCavityByName(cavityName);
							dspCavities.Add(c);
						}
						if (nextstep.Tip3Dsp != 0)
						{
							string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
								nextstep.Tip3Dsp);
							c = workbench.Layout.GetCavityByName(cavityName);
							dspCavities.Add(c);
						}
						workbench.AddIs(aspCavities, dspCavities);
						nap.CurrentStep = 3;
						nap.IsCurrentStepFinished = 0;
						break;
						case 4:
						aspCavities.Clear();
						dspCavities.Clear();
						if (nextstep.Tip1Asp != 0)
						{
							string cavityName = string.Format("RD_ISBottole{0}:RD_Bottole_Cavity1", nextstep.Tip1Asp);
							c = workbench.Layout.GetCavityByName(cavityName);
							aspCavities.Add(c);
						}
						if (nextstep.Tip2Asp != 0)
						{
							string cavityName = string.Format("RD_ISBottole{0}:RD_Bottole_Cavity1", nextstep.Tip2Asp);
							c = workbench.Layout.GetCavityByName(cavityName);
							aspCavities.Add(c);
						}
						if (nextstep.Tip3Asp != 0)
						{
							string cavityName = string.Format("RD_ISBottole{0}:RD_Bottole_Cavity1", nextstep.Tip3Asp);
							c = workbench.Layout.GetCavityByName(cavityName);
							aspCavities.Add(c);
						}
						if (nextstep.Tip1Dsp != 0)
						{
							string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
								nextstep.Tip1Dsp);
							c = workbench.Layout.GetCavityByName(cavityName);
							dspCavities.Add(c);
						}
						if (nextstep.Tip2Dsp != 0)
						{
							string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
								nextstep.Tip2Dsp);
							c = workbench.Layout.GetCavityByName(cavityName);
							dspCavities.Add(c);
						}
						if (nextstep.Tip3Dsp != 0)
						{
							string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
								nextstep.Tip3Dsp);
							c = workbench.Layout.GetCavityByName(cavityName);
							dspCavities.Add(c);
						}
						workbench.AddIs(aspCavities, dspCavities);
						nap.CurrentStep = 4;
						nap.IsCurrentStepFinished = 0;
						break;
						case 5:
						#region 加样

						aspCavities.Clear();
						dspCavities.Clear();

                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName;
                                if (nextstep.Tip1Asp > 80)
                                {
                                    cavityName = string.Format("RD_PNBottole{0}:RD_PNBottole_Cavity1",
                                        nextstep.Tip1Asp - 80);
                                }
                                else
                                {
                                    cavityName = string.Format("RD_SampleTube{0}:RD_SampleTube_Cavity1",
                                        nextstep.Tip1Asp);
                                }
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName;
                                if (nextstep.Tip2Asp > 80)
                                {
                                    cavityName = string.Format("RD_PNBottole{0}:RD_PNBottole_Cavity1",
                                        nextstep.Tip2Asp - 80);
                                }
                                else
                                {
                                    cavityName = string.Format("RD_SampleTube{0}:RD_SampleTube_Cavity1",
                                        nextstep.Tip2Asp);
                                }
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName;
                                if (nextstep.Tip3Asp > 80)
                                {
                                    cavityName = string.Format("RD_PNBottole{0}:RD_PNBottole_Cavity1",
                                        nextstep.Tip3Asp - 80);
                                }
                                else
                                {
                                    cavityName = string.Format("RD_SampleTube{0}:RD_SampleTube_Cavity1",
                                        nextstep.Tip3Asp);
                                }
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip1Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            workbench.AddSample(aspCavities, dspCavities, 400);

                            #endregion 加样

                            nap.CurrentStep = 5;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 6:

						#region 加样

						aspCavities.Clear();
						dspCavities.Clear();

                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName;
                                if (nextstep.Tip1Asp > 80)
                                {
                                    cavityName = string.Format("RD_PNBottole{0}:RD_PNBottole_Cavity1",
                                        nextstep.Tip1Asp - 80);
                                }
                                else
                                {
                                    cavityName = string.Format("RD_SampleTube{0}:RD_SampleTube_Cavity1",
                                        nextstep.Tip1Asp);
                                }
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName;
                                if (nextstep.Tip2Asp > 80)
                                {
                                    cavityName = string.Format("RD_PNBottole{0}:RD_PNBottole_Cavity1",
                                        nextstep.Tip2Asp - 80);
                                }
                                else
                                {
                                    cavityName = string.Format("RD_SampleTube{0}:RD_SampleTube_Cavity1",
                                        nextstep.Tip2Asp);
                                }
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName;
                                if (nextstep.Tip3Asp > 80)
                                {
                                    cavityName = string.Format("RD_PNBottole{0}:RD_PNBottole_Cavity1",
                                        nextstep.Tip3Asp - 80);
                                }
                                else
                                {
                                    cavityName = string.Format("RD_SampleTube{0}:RD_SampleTube_Cavity1",
                                        nextstep.Tip3Asp);
                                }
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip1Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            workbench.AddSample(aspCavities, dspCavities, 400);

                            #endregion 加样

                            nap.CurrentStep = 6;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 7:
                            workbench.MoveToShaker(nextstep.NapId);
                            nap.CurrentStep = 7;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 8:
                            workbench.Shake(nextstep.NapId, 30);
                            nap.CurrentStep = 8;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 9:
                            workbench.MoveToHeating(nextstep.NapId);
                            nap.CurrentStep = 9;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 10:
                            workbench.Wait(nextstep.NapId, 600);
                            nap.CurrentStep = 10;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 11:
                            workbench.MoveToCool(nextstep.NapId);
                            nap.CurrentStep = 11;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 12:
                            workbench.Wait(nextstep.NapId, 600);
                            nap.CurrentStep = 12;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 13:
                            workbench.MoveToMag(nextstep.NapId);
                            nap.CurrentStep = 13;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 14:
                            workbench.Wait(nextstep.NapId, 120);
                            nap.CurrentStep = 14;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 15:
                            aspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            workbench.FirstWaste(nextstep.NapId, aspCavities);
                            nap.CurrentStep = 15;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 16:
                            aspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            workbench.FirstWaste(nextstep.NapId, aspCavities);
                            nap.CurrentStep = 16;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 17:
                            workbench.MoveToCool(nextstep.NapId);
                            nap.CurrentStep = 17;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 18:
                            dspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip1Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            workbench.FirstWashing(dspCavities);
                            nap.CurrentStep = 18;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 19:
                            workbench.MoveToShaker(nextstep.NapId);
                            nap.CurrentStep = 19;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 20:
                            workbench.Shake(nextstep.NapId, 30);
                            nap.CurrentStep = 20;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 21:
                            workbench.MoveToMag(nextstep.NapId);
                            nap.CurrentStep = 21;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 22:
                            workbench.Wait(nextstep.NapId, 120);
                            nap.CurrentStep = 22;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 23:
                            aspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            workbench.SecondWaste(nextstep.NapId, aspCavities);
                            nap.CurrentStep = 23;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 24:
                            aspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            workbench.SecondWaste(nextstep.NapId, aspCavities);
                            nap.CurrentStep = 24;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 25:
                            workbench.MoveToCool(nextstep.NapId);
                            nap.CurrentStep = 25;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 26:
                            dspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip1Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            workbench.SecondWashing(dspCavities);
                            nap.CurrentStep = 26;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 27:
                            dspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip1Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            workbench.AddOil(dspCavities);
                            nap.CurrentStep = 27;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 28:
                            workbench.MoveToShaker(nextstep.NapId);
                            nap.CurrentStep = 28;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 29:
                            workbench.Shake(nextstep.NapId, 30);
                            nap.CurrentStep = 29;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 30:
                            workbench.MoveToMag(nextstep.NapId);
                            nap.CurrentStep = 30;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 31:
                            workbench.Wait(nextstep.NapId, 120);
                            nap.CurrentStep = 31;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 32:
                            aspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            workbench.LastWaste(nextstep.NapId, aspCavities);
                            nap.CurrentStep = 32;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 33:
                            aspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            workbench.LastWaste(nextstep.NapId, aspCavities);
                            nap.CurrentStep = 33;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 34:
                            workbench.MoveToCool(nextstep.NapId);
                            nap.CurrentStep = 34;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 35:
                            aspCavities.Clear();
                            dspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_AMPBottole{0}:RD_AMPBottole_Cavity1",
                                    nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_AMPBottole{0}:RD_AMPBottole_Cavity1",
                                    nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_AMPBottole{0}:RD_AMPBottole_Cavity1",
                                    nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip1Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            workbench.AddAmp(aspCavities, dspCavities);
                            nap.CurrentStep = 35;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 36:
                            aspCavities.Clear();
                            dspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_AMPBottole{0}:RD_AMPBottole_Cavity1",
                                    nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_AMPBottole{0}:RD_AMPBottole_Cavity1",
                                    nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_AMPBottole{0}:RD_AMPBottole_Cavity1",
                                    nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip1Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            workbench.AddAmp(aspCavities, dspCavities);
                            nap.CurrentStep = 36;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 37:
                            workbench.MoveToShaker(nextstep.NapId);
                            nap.CurrentStep = 37;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 38:
                            workbench.Shake(nextstep.NapId, 60);
                            nap.CurrentStep = 38;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 39:
                            workbench.MoveToHeating(nextstep.NapId);
                            nap.CurrentStep = 39;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 40:
                            dspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip1Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            workbench.AddParaffin(dspCavities);
                            nap.CurrentStep = 40;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 41:
                            workbench.Wait(nextstep.NapId, 600);
                            nap.CurrentStep = 41;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 42:
                            workbench.MoveToShaker(nextstep.NapId);
                            nap.CurrentStep = 42;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 43:
                            workbench.Shake(nextstep.NapId, 15);
                            nap.CurrentStep = 43;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 44:
                            workbench.MoveToReader(nextstep.NapId);
                            nap.CurrentStep = 44;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 45:
                            workbench.Wait(nextstep.NapId, 300);
                            nap.CurrentStep = 45;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 46:
                            aspCavities.Clear();
                            dspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_EzBottole{0}:RD_Bottole_Cavity1", nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_EzBottole{0}:RD_Bottole_Cavity1", nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_EzBottole{0}:RD_Bottole_Cavity1", nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip1Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            workbench.AddEz(aspCavities, dspCavities);
                            nap.CurrentStep = 46;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 47:
                            aspCavities.Clear();
                            dspCavities.Clear();
                            if (nextstep.Tip1Asp != 0)
                            {
                                string cavityName = string.Format("RD_EzBottole{0}:RD_Bottole_Cavity1", nextstep.Tip1Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip2Asp != 0)
                            {
                                string cavityName = string.Format("RD_EzBottole{0}:RD_Bottole_Cavity1", nextstep.Tip2Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip3Asp != 0)
                            {
                                string cavityName = string.Format("RD_EzBottole{0}:RD_Bottole_Cavity1", nextstep.Tip3Asp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                aspCavities.Add(c);
                            }
                            if (nextstep.Tip1Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip1Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip2Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip2Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            if (nextstep.Tip3Dsp != 0)
                            {
                                string cavityName = string.Format("RD_Cup{0}:RD_Cup_Cavity{1}", nextstep.NapId,
                                    nextstep.Tip3Dsp);
                                c = workbench.Layout.GetCavityByName(cavityName);
                                dspCavities.Add(c);
                            }
                            workbench.AddEz(aspCavities, dspCavities);
                            nap.CurrentStep = 47;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 48:
                            workbench.Wait(nextstep.NapId, 2400);
                            nap.CurrentStep = 48;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 49:
                            workbench.MoveToCoolDown(nextstep.NapId);
                            nap.CurrentStep = 49;
                            nap.IsCurrentStepFinished = 0;
                            break;
                        case 50:
                            workbench.Wait(nextstep.NapId, 300);
                            nap.CurrentStep = 50;
                            nap.IsCurrentStepFinished = 1;
                            break;
                        case 51:
                            workbench.MoveToDrop(nextstep.NapId);
                            nap.CurrentStep = 51;
                            nap.IsCurrentStepFinished = 0;
                            break;
                    }
                }
            }
        }

        public void ShakeFinished(int napId)
        {
            var nap = App.GlobalData.Naps.FirstOrDefault(n => n.NapId == napId);
            nap.IsCurrentStepFinished = 0;
        }

        public void WaitFinished(int napId)
        {
            var nap = App.GlobalData.Naps.FirstOrDefault(n => n.NapId == napId);
            nap.IsCurrentStepFinished = 0;
        }

        //public void Test()
        //{
        //	RdData rdData = new RdData();
        //	Sample sample = rdData.Samples.FirstOrDefault(s => s.SmpPos == 1);
        //	sample.SmpId = 1;
        //	sample.Sex = Sex.Female;
        //	sample.SamplingDate = DateTime.Now;
        //	sample.SampleType = "11";
        //	sample.PatientName = "aa";
        //	sample.IsWaitingForTake = 1;
        //	sample.IsEmergency = 0;
        //	sample.IsDoItem4 = 1;
        //	sample.IsDoItem3 = 1;
        //	sample.IsDoItem2 = 1;
        //	sample.IsDoItem1 = 1;
        //	sample.Id = 1;
        //	sample.Age = 1;
        //	sample.Barcode = "12121";
        //	//ICavity c = workbench.Layout.GetCavityByName("RD_SampleTube1:RD_SampleTube_Cavity1");
        //	// sample.Cavity = c;

		//	CalcNext0606.Class1 c1 = new Class1();
		//	int[,] arrarySample = rdData.ToSampleArray();

		//	MWNumericArray mwArraySample = new MWNumericArray(MWArrayComplexity.Real, 88, 7);
		//	for (int i = 1; i <= 88; i++)
		//	{
		//		for (int j = 1; j <= 7; j++)
		//		{
		//			mwArraySample[i, j] = arrarySample[i - 1, j - 1];
		//		}
		//	}
		//	for (int i = 1; i <= 21; i++)
		//	{
		//		Nap nap = new Nap();
		//		nap.NapId = i;
		//		nap.CurrentStep = i;
		//		nap.IsCurrentStepFinished = 0;
		//		nap.IsCurrentNapExist = 1;
		//		rdData.Naps.Add(nap);
		//	}
		//	int[,] arrayNap = rdData.ToNapArray();

		//	MWNumericArray mwArrayNap = new MWNumericArray(MWArrayComplexity.Real, 21, 4);
		//	for (int i = 1; i <= 21; i++)
		//	{
		//		for (int j = 1; j <= 4; j++)
		//		{
		//			mwArrayNap[i, j] = arrayNap[i - 1, j - 1];
		//		}
		//	}
		//	MWArray smpInput = mwArraySample;
		//	MWArray cupInput = mwArrayNap;

		//	MWArray mwArrayNext = c1.CalcNext0606(smpInput, cupInput);

		//	Int16[,] intArrayNext = (Int16[,])mwArrayNext.ToArray();

		//	rdData.FromArray(intArrayNext);
		//}

		/// <summary>
		/// 打开Shaker
		/// </summary>
		/// <param name="time">时间：秒</param>
		/// <param name="direction">方向：0=顺时针    1=逆时针</param>
		/// <returns></returns>
		public ErrCode OpenShaker(int time, int direction = 0)
		{
			return workbench.OpenShaker(time, direction);
		}

        /// <summary>
        /// 关闭Shaker
        /// </summary>
        /// <returns></returns>
        public ErrCode CloseShaker()
        {
            return workbench.CloseShaker();
        }

        private int t = 1;

		public void CalcNext()
		{
			CalcNext0606.Class1 c1 = new CalcNext0606.Class1();
			int[,] arrarySample = App.GlobalData.ToSampleArray();

            MWNumericArray mwArraySample = new MWNumericArray(MWArrayComplexity.Real, 88, 7);
            for (int i = 1; i <= 88; i++)
            {
                for (int j = 1; j <= 7; j++)
                {
                    mwArraySample[i, j] = arrarySample[i - 1, j - 1];
                }
            }

            int[,] arrayNap = App.GlobalData.ToNapArray();

			MWNumericArray mwArrayNap = new MWNumericArray(MWArrayComplexity.Real, 21, 4);
			for (int i = 1; i <= 21; i++)
			{
				for (int j = 1; j <= 4; j++)
				{
					mwArrayNap[i, j] = arrayNap[i - 1, j - 1];
				}
			}
			MWArray smpInput = mwArraySample;
			MWArray cupInput = mwArrayNap;

            System.Diagnostics.Debug.WriteLine(smpInput.ToArray()+"\n\n\n");
            System.Diagnostics.Debug.WriteLine(cupInput.ToArray() + "\n\n\n");

            Console.WriteLine(string.Format("第{0}次：",t));
            RDS.SDK.ConsoleManager.Write(arrarySample);
            Console.WriteLine("\n\n");
            RDS.SDK.ConsoleManager.Write(arrayNap);
            Console.WriteLine("\n\n");

            MWArray mwArrayNext = c1.CalcNext0606(smpInput, cupInput);
            System.Diagnostics.Debug.WriteLine(mwArrayNext.ToArray() + "\n\n\n");
            Int16[,] intArrayNext = (Int16[,]) mwArrayNext.ToArray();
            RDS.SDK.ConsoleManager.Write(intArrayNext);
            Console.WriteLine("\n\n");
            App.GlobalData.FromArray(intArrayNext);
            t++;
        }
    }
}