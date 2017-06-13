using RDS.ViewModels.Common;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDS.ViewModels.ViewProperties;
using System.Collections;
using RDS.Apps;

namespace RDS.ViewModels
{
	public class StripViewModel : ViewModel
	{
		private bool[] isUsed = new bool[21];

		private int usedCount;
		public int UsedCount
		{
			get { return usedCount; }
			set
			{
				usedCount = value;
				this.RaisePropertyChanged(nameof(UsedCount));
			}
		}

		public int SelectedUsedCount { get; private set; }

		public int UnSelectedUsedCount
		{
			get
			{
				var unSelectedUsedCount = this.UsedCount - this.SelectedUsedCount;
				if (unSelectedUsedCount < 0) unSelectedUsedCount = 0;
				return unSelectedUsedCount;
			}
		}

		public ObservableCollection<CupRack> CupRacks { get; set; }

		public enum ViewChangedOption
		{
			ExitView = 0
		}

		public class StripViewChangedArgs : EventArgs
		{
			public ViewChangedOption Option { get; set; }
			public object Value { get; set; }

			public StripViewChangedArgs(ViewChangedOption option, object value)
			{
				this.Option = option;
				this.Value = value;
			}
		}

		public RelayCommand ExitView { get; private set; }

		public StripViewModel()
		{
			//this.UsedCount = App.GlobalData.UsedNapCount;
			this.ExitView = new RelayCommand(this.ExecuteExitView,this.CanExecuteExitView);
		}

		private void ExecuteExitView()
		{
			this.SaveStripIsUsed(this.UsedCount);
			this.OnViewChanged(new StripViewChangedArgs(ViewChangedOption.ExitView, null));
		}

		private bool CanExecuteExitView()
		{
			return this.SelectedUsedCount>=this.UsedCount ? true : false;
		}

		public void InitializeStripView(object value)
		{
			this.CupRacks = (ObservableCollection<CupRack>)value;
			this.RaisePropertyChanged(nameof(this.CupRacks));
			this.RaisePropertyChanged(nameof(this.UnSelectedUsedCount));
		}

		public void SetStripState(int stripIndex)
		{
			var a = stripIndex / 7;
			var b = stripIndex % 7;
			var strip = this.CupRacks[a].Strips[b];
			strip.IsLoaded = !strip.IsLoaded;
			var number = strip.Number;
			var nap = Apps.App.GlobalData.Naps.FirstOrDefault(o => o.CurrentPos == number);
			nap.IsCurrentNapExist = strip.IsLoaded == true ? 1 : 0;
		}

		public void UpdateSelectedUsedCount(int stripIndex)
		{
			this.isUsed[stripIndex] = !this.isUsed[stripIndex];
			this.SelectedUsedCount = this.isUsed.ToList().Where(o => o == true).Count();
			this.RaisePropertyChanged(nameof(this.SelectedUsedCount));
			this.RaisePropertyChanged(nameof(this.UnSelectedUsedCount));
			this.ExitView.RaiseCanExecuteChanged();
		}

		private void SaveStripIsUsed(int usedStripCounts)
		{
			var naps = App.GlobalData.Naps.ToArray();
			for (int i = 0; i < this.CupRacks.Count; i++)
			{
				for (int j = 0; j < this.CupRacks[i].Strips.Count; j++)
				{
					if (usedStripCounts > 0)
					{
						if (naps[i * 7 + j].IsCurrentNapExist == 1)
						{
							naps[i * 7 + j].IsCurrentNapUsed = 1;
							usedStripCounts--;
						}
					}
					else break;
				}
			}
		}
	}
}
