using System.Linq;

namespace RDS.Models
{
	public class SampleInformation : ViewModels.Common.ViewModel
	{
		public SampleInformation() { }

		public string SampleId { get; set; } = string.Empty;

		public string Barcode { get; set; } = string.Empty;

		private string name = string.Empty;
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				this.RaisePropertyChanged(nameof(Name));
			}
		}

		public string Age { get; set; } = string.Empty;

		public string Sex { get; set; } = string.Empty;

		public string Type { get; set; } = string.Empty;

		private string holeName = string.Empty;
		public string HoleName
		{
			get { return holeName; }
			set
			{
				holeName = value;
				this.RaisePropertyChanged(nameof(HoleName));
			}
		}

		public string Birthday { get; set; } = string.Empty;

		public string Reagent { get; set; } = string.Empty;

		public string DateTime { get; set; } = string.Empty;

		private bool isEmergency = false;
		public bool IsEmergency
		{
			get { return isEmergency; }
			set
			{
				isEmergency = value;
				this.RaisePropertyChanged(nameof(IsEmergency));
			}
		}


		public int[] GetReagentItems(System.Collections.Generic.ICollection<string> usedReagents)
		{
			var result = new int[4];
			var reagentItems = new System.Collections.Generic.List<string>(this.Reagent.Split(Properties.Resources.Separator4.ToCharArray()[0]));
			for (int i = 0; i < usedReagents.Count; i++)
			{
				var currentItem = usedReagents.ToArray()[i];
				var hasItem = reagentItems.FirstOrDefault(o => o == currentItem);
				result[i] = hasItem != null ? 1 : 0;
			}
			return result;
		}
	}
}
