using RDS.Models;
using RDS.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDS.ViewModels
{
	public class SetupViewModel : ViewModel
	{
		private string passwordMessage = "请输入密码";
		public string PasswordMessage
		{
			get { return passwordMessage; }
			set
			{
				passwordMessage = value;
				this.RaisePropertyChanged(nameof(PasswordMessage));
			}
		}

		private string newReagentSerierName;
		public string NewReagentSerierName
		{
			get { return newReagentSerierName; }
			set
			{
				newReagentSerierName = value;
				this.RaisePropertyChanged(nameof(NewReagentSerierName));
			}
		}

		private string newReagentItemName;
		public string NewReagentItemName
		{
			get { return newReagentItemName; }
			set
			{
				newReagentItemName = value;
				this.RaisePropertyChanged(nameof(NewReagentItemName));
			}
		}

		private int viewIndex;
		public int ViewIndex
		{
			get { return viewIndex; }
			set
			{
				viewIndex = value;
				this.RaisePropertyChanged(nameof(ViewIndex));
			}
		}

		public ObservableCollection<Language> Languages { get; set; } = new ObservableCollection<Language>();

		public Language SelectedLanguage { get; set; }

		public ObservableCollection<ReagentSeries> ReagentSeries { get; set; } = new ObservableCollection<ReagentSeries>();

		public List<string> UsedReagents { get; private set; } = new List<string>();

		public RelayCommand RemoveReagentInformation { get; private set; }

		public RelayCommand AddReagentItem { get; private set; }

		public RelayCommand AddReagentSeries { get; private set; }

		public RelayCommand SaveConfigurationation { get; private set; }

		public RelayCommand ValidateAdministrators { get; private set; }

		public enum ViewChangedOption
		{
			ValidateAdministrators=0
		}

		public class SetupViewChangedArgs : EventArgs
		{
			public ViewChangedOption Option { get; set; }
			public object Value { get; set; }

			public SetupViewChangedArgs(ViewChangedOption option, object value)
			{
				this.Option = option;
				this.Value = value;
			}
		}

		public SetupViewModel()
		{
			this.RemoveReagentInformation = new RelayCommand(this.ExecuteRemoveReagentInformation);

			this.AddReagentItem = new RelayCommand(this.ExecuteAddReagentItem);

			this.AddReagentSeries = new RelayCommand(this.ExecuteAddReagentSeries);

			this.SaveConfigurationation = new RelayCommand(this.ExecuteSaveConfiguration);

			this.ValidateAdministrators = new RelayCommand(this.ExecuteValidateAdministrators);

			this.InitializeReagentInformation();

			this.InitializeLanguages();
		}

		private void InitializeLanguages()
		{
			var languageInformation = General.ReadConfiguration(Properties.Resources.LanguageInformation).Split(Properties.Resources.Separator2.ToCharArray()[0]);
			for (int i = 0; i < languageInformation.Length; i++)
			{
				this.Languages.Add(new Language(languageInformation[i]));
			}
			this.SelectedLanguage = this.Languages.FirstOrDefault(o => o.IsUsed == true);
		}

		private void SaveLanguage()
		{
			var resultArray = new string[this.Languages.Count];
			for (int i = 0; i < this.Languages.Count; i++)
			{
				if (this.Languages[i].Name == this.SelectedLanguage.Name) this.Languages[i].IsUsed = true;
				else this.Languages[i].IsUsed = false;
				resultArray[i] = this.Languages[i].Serialize();
			}
			var result = string.Join(Properties.Resources.Separator2, resultArray);
			General.WriteConfiguration(Properties.Resources.LanguageInformation, result);
		}

		private void InitializeReagentInformation()
		{
			var reagentConfigationInformation = General.ReadConfiguration(Properties.Resources.ReagentInformation).Split(Properties.Resources.Separator1.ToCharArray()[0]).ToList();
			for (int i = 0; i < reagentConfigationInformation.Count; i++)
			{
				var reagentItemsList = reagentConfigationInformation[i].Split(Properties.Resources.Separator2.ToCharArray()[0]).ToList();
				var reagentItems = new ObservableCollection<ReagentItem>();
				for (int j = 1; j < reagentItemsList.Count; j++)
				{
					var item = reagentItemsList[j].Split(Properties.Resources.Separator3.ToCharArray()[0]);
					if (item[1] == Properties.Resources.One) this.UsedReagents.Add(item[0]);
					var itemName = item[0];
					reagentItems.Add(new ReagentItem(reagentItemsList[0], itemName, item[1] == Properties.Resources.One ? true : false));
				}
				this.ReagentSeries.Add(new ReagentSeries(reagentItemsList[0], reagentItems));
			}
		}

		private void SaveReagentInformation()
		{
			this.UsedReagents.Clear();
			var seriesCount = this.ReagentSeries.Count;
			string[] result = new string[seriesCount];
			StringBuilder[] seriesTemp = new StringBuilder[seriesCount];
			for (int i = 0; i < seriesCount; i++)
			{
				var currentSeries = this.ReagentSeries[i];
				seriesTemp[i] = new StringBuilder();
				seriesTemp[i].AppendFormat(Properties.Resources.StringFormat2, currentSeries.Name,Properties.Resources.Separator2);
				var items = new string[currentSeries.ReagentItems.Count];
				for (int j = 0; j < currentSeries.ReagentItems.Count; j++)
				{
					var currentItem = currentSeries.ReagentItems[j];
					items[j] = string.Format
					(
						Properties.Resources.StringFormat3,
						currentItem.Name,Properties.Resources.Separator3, currentItem.IsUsed ? Properties.Resources.One : Properties.Resources.Zero
					);
					if (currentItem.IsUsed) this.UsedReagents.Add(currentItem.Name);
					
				}
				seriesTemp[i].Append(string.Join(Properties.Resources.Separator2, items));
				result[i] = seriesTemp[i].ToString();
			}
			General.WriteConfiguration(Properties.Resources.ReagentInformation, string.Join(Properties.Resources.Separator1, result));
		}

		public void ExecuteSaveConfiguration()
		{
			this.SaveLanguage();

			this.SaveReagentInformation();
		}

		/// <summary>
		/// remove one ReagentSeries or ReagentItem object   
		/// </summary>
		/// <param name="param">selected object</param>
		public void ExecuteRemoveReagentInformation(object param)
		{
			if (param is ReagentSeries) this.ReagentSeries.Remove((ReagentSeries)param);
			else if (param is ReagentItem)
			{
				var reagentItem = (ReagentItem)param;
				var reagentSeries = this.ReagentSeries.FirstOrDefault(o => o.Name == reagentItem.ParentName);
				reagentSeries.ReagentItems.Remove(reagentItem);
			}
		}

		/// <summary>
		/// add new ReagentItem
		/// </summary>
		/// <param name="param">parent object of the ReagentItem</param>
		private void ExecuteAddReagentItem(object param)
		{
			var reagentSeries = (ReagentSeries)param;
			reagentSeries.ReagentItems.Add(new ReagentItem(reagentSeries.Name, this.NewReagentItemName, false));
			this.NewReagentItemName = string.Empty;
		}

		private void ExecuteAddReagentSeries()
		{
			this.ReagentSeries.Add(new ReagentSeries(this.NewReagentSerierName, new ObservableCollection<ReagentItem>()));
			this.NewReagentSerierName = string.Empty;
		}

		private void ExecuteValidateAdministrators()
		{
			this.OnViewChanged(new SetupViewChangedArgs(ViewChangedOption.ValidateAdministrators, Properties.Resources.Password));
		}
	}
}
