using RDS.ViewModels.Common;
using System.Windows.Media;

namespace RDS.ViewModels.ViewProperties
{
	public class Reagent : ViewModel
	{
		private int alarmVolume = 0;

		private int volume = 0;
		public int Volume
		{
			get { return volume; }
			set
			{
				if (value >= 0 && value <= 100)
				{
					volume = value;
					this.RaisePropertyChanged(nameof(Volume));
					if (value <= this.alarmVolume && value > 0) this.IsTwinkle = true;
					else this.IsTwinkle = false;

					if (value > 0) this.Color = General.WathetColor;
					//if (value > 0) this.Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE0E0E0"));
					else this.Color = new SolidColorBrush(Colors.White);

					this.RaisePropertyChanged(nameof(this.IsTwinkle));
					this.RaisePropertyChanged(nameof(this.Color));
					this.RaisePropertyChanged(nameof(this.Content));
				}
			}
		}

		public bool IsTwinkle { get; set; }

		public SolidColorBrush Color { get; set; }

		private string name=string.Empty;
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				this.RaisePropertyChanged(nameof(Name));
				this.RaisePropertyChanged(nameof(this.Content));
			}
		}


		public string Content { get { return string.Format(Properties.Resources.StringFormat3, this.Name,Properties.Resources.Separator3, this.Volume); } }

		public Reagent(string name, int volume = 0, int alarmVolume = 0)
		{
			this.name = name;
			this.Volume = volume;
			this.alarmVolume = alarmVolume;
		}
	}
}
