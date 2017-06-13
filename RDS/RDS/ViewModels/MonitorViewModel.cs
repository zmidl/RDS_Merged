using RDS.ViewModels.Common;
using System.Collections.ObjectModel;
using RDS.ViewModels.ViewProperties;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;

namespace RDS.ViewModels
{
	public class MonitorViewModel : ViewModel
	{
		private const int CUPRACKS_COUNT = 3;
		private const int TIPRACKS_COUNT = 9;
		public bool samplingResult;

		public string RemainingTime { get { return remainingTime.ToString(Properties.Resources.RemainingTimeFormat); } }

		private readonly int yearUnit = 1;
		private readonly int monthUnit = 1;
		private readonly int dateUnit = 1;
		private int hourUnit = 5;
		private int minuteUnit = 0;
		private int secondUnit = 0;

		private DateTime remainingTime;

		private System.Timers.Timer remainingTimer;

		private ObservableCollection<Strip>[] StripGroups = new ObservableCollection<Strip>[7];

		public StripViewModel StripViewModel { get; set; }

		public SampleViewModel SampleViewModel { get; set; }

		public ObservableCollection<CupRack> CupRacks { get; set; } = new ObservableCollection<CupRack>();

		public ObservableCollection<TipRack> TipRacks { get; set; } = new ObservableCollection<TipRack>();

		public ReagentRack ReagentRack { get; set; } = new ReagentRack();

		public Heating Heating { get; set; } = new Heating();

		public ShakerRack ShakerRack { get; set; } = new ShakerRack();

		public Mag Mag { get; set; } = new Mag();

		public Reader Reader { get; set; } = new Reader();

		private Visibility circleVisibility = Visibility.Collapsed;
		public Visibility CircleVisibility
		{
			get { return circleVisibility; }
			set
			{
				circleVisibility = value;
				this.RaisePropertyChanged(nameof(CircleVisibility));
			}
		}

		private string startTask = "开始任务";
		public string StartTask
		{
			get { return startTask; }
			set
			{
				startTask = value;
				this.RaisePropertyChanged(nameof(StartTask));
			}
		}

		private bool isStartTask;
		public bool IsStartTask
		{
			get { return isStartTask; }
			set
			{
				if (value)
				{
					if (this.samplingResult == true)
					{
						this.StartRemainingTimer();
						isStartTask = value;
						this.RaisePropertyChanged(nameof(IsStartTask));
						this.StartTask = "任务进行中...";
						this.CircleVisibility = Visibility.Visible;
						//Task.Factory.StartNew(() => {this.Sdk.LoopExecute(); });
					}
					else General.PopupWindow(PopupType.OneButton, General.FindStringResource(Properties.Resources.PopupWindow_Message2), null);
				}
				else
				{
					this.StopRemainingTimer();
					isStartTask = value;
					this.RaisePropertyChanged(nameof(IsStartTask));
					this.StartTask = "开始任务";
					this.CircleVisibility = Visibility.Collapsed;
				}
			}
		}

		public enum ViewChangedOption
		{
			ShowSampleView = 0,
			ShowStripView = 1,
			TaskStop = 2,
			NotifySamplingResult = 3
		}

		public class MonitorViewChangedArgs : EventArgs
		{
			public ViewChangedOption Option { get; set; }
			public object Value { get; set; }

			public MonitorViewChangedArgs(ViewChangedOption option, object value)
			{
				this.Option = option;
				this.Value = value;
			}
		}

		public RelayCommand Emergency { get; private set; }

		public MonitorViewModel()
		{
			this.InitializeSdk();

			this.StripViewModel = new StripViewModel();

			this.SampleViewModel = new SampleViewModel();

			this.SampleViewModel.SamplingResult = new Action<bool>(this.SetSamplingResult);

			this.InitializeCupRacks(MonitorViewModel.CUPRACKS_COUNT);

			this.InitializeTipRacks(MonitorViewModel.TIPRACKS_COUNT);

			this.Emergency = new RelayCommand(this.ShowSampleView);

			this.InitializeRemainingTimer();

			this.remainingTime = new DateTime(yearUnit, monthUnit, dateUnit, hourUnit, minuteUnit, secondUnit);

			this.InitializeStripGroups();

			this.InitializeReagentRack();
		}

		private void Workbench_RefreshUiEvent(RdCore.Enum.RdAction action, object obj1, object obj2)
		{
			//throw new NotImplementedException();
		}

		private void InitializeReagentRack()
		{
			var reagents = General.UsedReagents;

			this.ReagentRack.MBBottles[0].Name = "磁珠";
			this.ReagentRack.MBBottles[1].Name = "磁珠";

			for (int i = 0; i < reagents.Count; i++)
			{
				this.ReagentRack.AMPBottles[i].Name = reagents[i];
				this.ReagentRack.PNBottles[i].Name = reagents[i];
				this.ReagentRack.ISBottles[i].Name = reagents[i];
				this.ReagentRack.PNBottles[4+i].Name = reagents[i];
			}


		}

		private void InitializeSdk()
		{
			//this.Sdk = new Sdk();
			//this.Sdk.workbench.RefreshUiEvent += Workbench_RefreshUiEvent;
			//this.Sdk.EventHandler += Sdk_EventHandler;
			General.SDK.workbench.RefreshUiEvent+= Workbench_RefreshUiEvent;
			General.SDK.EventHandler+= Sdk_EventHandler;
		}

		private void Sdk_EventHandler(object sender, SDK.SdkMessage.SdkMessageArgs e)
		{
			e.Window = General.PopupWindow(e.PopupType, e.Message, e.Actions);
		}

		private void InitializeStripGroups()
		{
			this.StripGroups[0] = this.CupRacks[0].Strips;
			this.StripGroups[1] = this.CupRacks[1].Strips;
			this.StripGroups[2] = this.CupRacks[2].Strips;
			this.StripGroups[3] = this.Heating.Strips;
			this.StripGroups[4] = this.ShakerRack.Strips;
			this.StripGroups[5] = this.Mag.Strips;
			this.StripGroups[6] = this.Reader.Strips;
		}

		private void InitializeTipRacks(int tipRacksCount)
		{
			for (int i = 0; i < tipRacksCount; i++)
			{
				this.TipRacks.Add(new TipRack());
			}
		}

		private void InitializeCupRacks(int cupRacksCount)
		{
			for (int i = 0; i < cupRacksCount; i++)
			{
				this.CupRacks.Add(new CupRack(i));
			}
		}

		public void ShowSampleView()
		{
			this.OnViewChanged(new MonitorViewChangedArgs(ViewChangedOption.ShowSampleView, null));
		}

		public void ShowStripView()
		{
			this.OnViewChanged(new MonitorViewChangedArgs(ViewChangedOption.ShowStripView,this.CupRacks));
		}

		public void FinishTask()
		{
			this.OnViewChanged(new MonitorViewChangedArgs(ViewChangedOption.TaskStop, null));
		}

		public void SetSamplingResult(bool samplingResult)
		{
			this.samplingResult = samplingResult;
			this.OnViewChanged(new MonitorViewChangedArgs(ViewChangedOption.NotifySamplingResult, samplingResult));
		}

		public void SetSampleState(int twentyUnionSampleIndex, int sampleIndex, bool isLoaded)
		{
			this.SampleViewModel.FourSampleRackDescriptions[twentyUnionSampleIndex].Samples[sampleIndex].IsLoaded = isLoaded;
		}

		public void SetCupRackMixtureState(int cupRacksIndex, int stripsIndex, int mixtureIndex, bool isLoaded)
		{
			this.CupRacks[cupRacksIndex].Strips[stripsIndex].Cells[mixtureIndex].IsLoaded = isLoaded;
		}

		public void SetCupRackStripState(int cupRacksIndex, int stripsIndex, bool isLoaded, int number = 0)
		{
			var strip = this.CupRacks[cupRacksIndex].Strips[stripsIndex];
			strip.IsLoaded = isLoaded;
			strip.Number = number;
		}

		public void SetTipState(int tipRacksIndex, int tipsIndex, bool isLoaded)
		{
			this.TipRacks[tipRacksIndex].Tips[tipsIndex].IsLoaded = isLoaded;
		}

		public void SetTipRackState(int tipRackIndex)
		{
			if (this.TipRacks[tipRackIndex].IsLoaded == true) this.TipRacks[tipRackIndex].IsLoaded = false;
			else
			{
				if (tipRackIndex > 0) this.TipRacks[tipRackIndex].TipType = RDSCL.TipType._1000uLStyle;
				else this.TipRacks[tipRackIndex].TipType = RDSCL.TipType._300uLStyle;
				this.TipRacks[tipRackIndex].IsLoaded = true;
			}
			// 设置TipRack信息写到Skd.FillTip
		}

		public void SetReaderEnzymeBottleVolume(int enzymeIndex, int volume)
		{
			this.Reader.EnzymeBottles[enzymeIndex].Volume += volume;
		}

		public void SetReaderTemperature(int temperature)
		{
			this.Reader.Temperature = temperature;
		}

		public void SetSampleRackState(int sampleRackIndex, RDSCL.SampleRackState sampleRackState)
		{
			this.SampleViewModel.FourSampleRackDescriptions[sampleRackIndex].SampleRackState = sampleRackState;
		}

		public void SetOlefinBoxVolume(int volume)
		{
			this.Heating.OlefinBox.Volume = volume;
		}

		public void SetHeatingTemperature(int temperature)
		{
			this.Heating.Temperature = temperature;
		}

		public void SetReagentBoxVolume(int reagentBoxIndex, int volume)
		{
			this.ReagentRack.ReagentBoxs[reagentBoxIndex].Volume = volume;
		}

		public void SetMBBottleVolume(int mBBottleIndex, int volume)
		{
			this.ReagentRack.MBBottles[mBBottleIndex].Volume = volume;
		}

		public void InitializeRemainingTimer()
		{
			this.remainingTimer = new System.Timers.Timer(1000);

			this.remainingTimer.Elapsed += Timer_Elapsed;

			this.remainingTimer.AutoReset = true;
		}

		public void StartRemainingTimer()
		{
			if (remainingTime.ToString(Properties.Resources.RemainingTimeFormat) == Convert.ToDateTime(Properties.Resources.TimeOut).ToString(Properties.Resources.RemainingTimeFormat)) this.remainingTime = new DateTime(yearUnit, monthUnit, dateUnit, hourUnit, minuteUnit, secondUnit);
			this.remainingTimer.Enabled = true;
			this.remainingTimer.Start();
		}

		public void StopRemainingTimer()
		{
			this.remainingTimer.Stop();
			this.OnViewChanged(new MonitorViewChangedArgs(ViewChangedOption.TaskStop, null));
		}

		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (remainingTime.ToString(Properties.Resources.RemainingTimeFormat) != Convert.ToDateTime(Properties.Resources.TimeOut).ToString(Properties.Resources.RemainingTimeFormat))
			{
				remainingTime = remainingTime.AddSeconds(-1);
				this.RaisePropertyChanged(nameof(RemainingTime));
			}
			else
			{
				this.IsStartTask = false;
			}
		}

		public void StripMoved(int from, int to)
		{
			var pointFrom = this.GetStripLocation(from);
			var pointTo = this.GetStripLocation(to);
			this.StripGroups[pointTo.Item1][pointTo.Item2] = this.StripGroups[pointFrom.Item1][pointFrom.Item2];
			this.StripGroups[pointTo.Item1][pointTo.Item2].IsMoving = false;
			this.StripGroups[pointFrom.Item1][pointFrom.Item2] = new Strip();
		}

		public void StripMoving(int from, int to)
		{
			var pointFrom = this.GetStripLocation(from);
			var pointTo = this.GetStripLocation(to);
			this.StripGroups[pointFrom.Item1][pointFrom.Item2].IsMoving = true;
			this.StripGroups[pointTo.Item1][pointTo.Item2].IsMoving = true;
		}

		private Tuple<int, int> GetStripLocation(int index)
		{
			if (index < 1) index = 1;
			else if (index > 37) index = 37;
			int tenth = 0;
			int units = 0;
			if (index > 32) { tenth = 6; units = index % 33; }
			else if (index > 28) { tenth = 5; units = index % 29; }
			else if (index > 25) { tenth = 4; units = index % 26; }
			else if (index > 21) { tenth = 3; units = index % 22; }
			else if (index > 14) { tenth = 2; units = index % 15; }
			else if (index > 7) { tenth = 1; units = index % 8; }
			else { tenth = 0; units = index - 1; }
			return new Tuple<int, int>(tenth, units);
		}
	}
}