using RDS.ViewModels.Common;
using Sias.Core.Attributes;
using System.Collections.ObjectModel;

namespace RDS.ViewModels.ViewProperties
{
	public class TipRack:ViewModel
	{
		private const int TIPRACK_SIZE = 96;

		public ObservableCollection<Tip> Tips { get; set; } = new ObservableCollection<Tip>();

		private bool? isLoaded;
		public bool? IsLoaded
		{
			get { return isLoaded; }
			set
			{
				isLoaded = value;
				this.RaisePropertyChanged(nameof(IsLoaded));
			}
		}

		private bool isTwinkle;
		public bool IsTwinkle
		{
			get { return isTwinkle; }
			set
			{
				isTwinkle = value;
				this.RaisePropertyChanged(nameof(IsTwinkle));
			}
		}

		private RDSCL.TipType tipType;
		public RDSCL.TipType TipType
		{
			get { return tipType; }
			set
			{
				tipType = value;
				this.RaisePropertyChanged(nameof(TipType));
			}
		}

		public TipRack()
		{
			for (int i = 0; i < TipRack.TIPRACK_SIZE; i++)
			{
				this.Tips.Add(new Tip());
			}
		}
	}
}
