
using System.Collections.ObjectModel;
using RDS.Models;
using RDS.ViewModels.Common;
using RDS.ViewModels.ViewProperties;
using System.Data;
using System;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Text;
using RDSCL;

namespace RDS.ViewModels
{

	public class SampleViewModel : ViewModel
	{
		private Sdk Sdk;

		public Action<bool> SamplingResult { get; set; }

		public SampleRackIndex CurrentSampleRackIndex { get; set; } = 0;

		private readonly string[] columnNames = new string[4] { Properties.Resources.A, Properties.Resources.B, Properties.Resources.C, Properties.Resources.D };

		private DataTable lisInformationTable;

		public ObservableCollection<SampleRackDescription> FourSampleRackDescriptions { get; set; } = new ObservableCollection<SampleRackDescription>();

		public ObservableCollection<SampleInformation> CurrentSampleInformations { get; set; } = new ObservableCollection<SampleInformation>();

		private ObservableCollection<SampleInformation>[] FourSampleInformations = new ObservableCollection<SampleInformation>[4];

		public RelayCommand ExitSampleView { get; private set; }

		public RelayCommand SetMultipleEmergency { get; private set; }

		public enum ViewChangedOption
		{
			ExitView = 0
		}

		public class SampleViewChangedArgs : EventArgs
		{
			public ViewChangedOption Option { get; set; }
			public object Value { get; set; }

			public SampleViewChangedArgs(ViewChangedOption option, object value)
			{
				this.Option = option;
				this.Value = value;
			}
		}

		public SampleViewModel()
		{
			for (int i = 0; i < 4; i++)
			{
				this.FourSampleRackDescriptions.Add(new SampleRackDescription(i));
			}

			this.GetLisTableFromFile();
			this.SetMultipleEmergency = new RelayCommand(this.ExecuteSettingMultipleEmergency);
			this.ExitSampleView = new RelayCommand(this.ExecuteExitSampleView);
		}

		private void ExecuteExitSampleView(object obj)
		{
			var isOk = bool.Parse(obj.ToString());
			if (isOk) this.SamplingResult(isOk);
			if (isOk) this.SaveSampleInformation();
			this.OnViewChanged(new SampleViewChangedArgs(ViewChangedOption.ExitView,null));
			this.RollBackSampleRacksState(true, this.CurrentSampleRackIndex);
		}

		private void SaveSampleInformation()
		{
			var samples = Apps.App.GlobalData.Samples;
			for (int i = 0; i < 80; i++)
			{
				var quotient = i / 20;
				var remainder = i % 20;
				var currentSampleInformations = this.FourSampleInformations[quotient];
				var sample = samples.FirstOrDefault(o => o.SmpPos == i + 1);
				if (currentSampleInformations != null)
				{
					var currentInformation = currentSampleInformations[remainder];
					if (currentSampleInformations != null)
					{
						var reagentItems = currentInformation.GetReagentItems(General.UsedReagents);
						if (string.IsNullOrEmpty(currentInformation.Age) == false) sample.Age = int.Parse(currentInformation.Age);
						sample.Barcode = currentInformation.Barcode;
						sample.PatientName = currentInformation.Name;
						sample.Sex = currentInformation.Sex == Properties.Resources.Male ? RdEntity.Sex.Male : RdEntity.Sex.Female;
						sample.SampleType = currentInformation.Type;
						if (string.IsNullOrEmpty(currentInformation.Birthday) == false) sample.Birthday = DateTime.ParseExact(currentInformation.Birthday, Properties.Resources.LisFileNameFormat, System.Globalization.CultureInfo.CurrentCulture);
						sample.IsDoItem1 = reagentItems[0];
						sample.IsDoItem2 = reagentItems[1];
						sample.IsDoItem3 = reagentItems[2];
						sample.IsDoItem4 = reagentItems[3];
						if (string.IsNullOrEmpty(currentInformation.DateTime) == false) sample.SamplingDate = DateTime.ParseExact(currentInformation.DateTime, "yyyyMMddhhmmss", System.Globalization.CultureInfo.CurrentCulture);
						sample.IsEmergency = 0;
					}
				}
			}

			for (int i = 0; i < 4; i++)
			{
				var isDoItem = string.IsNullOrEmpty(General.UsedReagents[i]) == false ? 1 : 0;

				var sample = samples.FirstOrDefault(o => o.SmpPos == 81 + i);
				switch (i)
				{
					case 0: { sample.IsDoItem1 = isDoItem; break; }
					case 1: { sample.IsDoItem2 = isDoItem; break; }
					case 2: { sample.IsDoItem3 = isDoItem; break; }
					case 3: { sample.IsDoItem4 = isDoItem; break; }
					default: break;
				}
				sample.IsEmergency = 1;

				sample = samples.FirstOrDefault(o => o.SmpPos == 85 + i);
				switch (i)
				{
					case 0: { sample.IsDoItem1 = isDoItem; break; }
					case 1: { sample.IsDoItem2 = isDoItem; break; }
					case 2: { sample.IsDoItem3 = isDoItem; break; }
					case 3: { sample.IsDoItem4 = isDoItem; break; }
					default: break;
				}
				sample.IsEmergency = 1;
			}
		}

		private void ExecuteSettingMultipleEmergency()
		{
			if (this.CurrentSampleInformations != null && this.CurrentSampleInformations.Count > 0)
			{
				var targetValue = false;
				if (this.CurrentSampleInformations.Where(o => o.IsEmergency == true).Count() == 0) targetValue = true;
				for (int i = 0; i < 20; i++) this.CurrentSampleInformations[i].IsEmergency = targetValue;
			}
		}

		private string GetHoleNameByNumber(int number)
		{
			var result = new StringBuilder();
			var quotient = number / 20;
			var remainder = number % 20;
			if (remainder == 0) { remainder = 20; quotient -= 1; }
			result.Append(columnNames[quotient]);
			result.Append(remainder);
			return result.ToString();
		}

		public void AssignSample()
		{
			var result = General.SDK.ReadBarcode((int)this.CurrentSampleRackIndex).Values.ToArray();
			SampleRackState sampleRackState = default(SampleRackState);
			if (result != null)
			{
				sampleRackState = SampleRackState.AlreadySample;
				this.ReadSampleInformationToGrid(this.CurrentSampleRackIndex, result);
			}
			else
			{
				sampleRackState = SampleRackState.NotSample;
			}
			this.FourSampleRackDescriptions[(int)this.CurrentSampleRackIndex].SampleRackState = sampleRackState;
		}

		public void ReadSampleInformationToGrid(SampleRackIndex sampleColumn, string[] result)
		{
			var sampleInformationsColumns = new ObservableCollection<SampleInformation>();
			if (this.lisInformationTable != null)
			{
				for (int i = 0; i < result.Length; i++)
				{
					var sampleInformation = this.GetSampleInformationFromLis(result[i]);
					sampleInformation.HoleName = this.GetHoleNameByNumber(i + 1 + (20 * (int)sampleColumn));
					sampleInformationsColumns.Add(sampleInformation);
				}
				this.CurrentSampleInformations = sampleInformationsColumns;
				this.FourSampleInformations[(int)sampleColumn] = sampleInformationsColumns;
				this.RaisePropertyChanged(nameof(this.CurrentSampleInformations));
			}
		}

		private SampleInformation GetSampleInformationFromLis(string barcode)
		{
			var result = new SampleInformation();
			for (int i = 0; i < this.lisInformationTable.Rows.Count; i++)
			{
				if (this.lisInformationTable.Rows[i][0].ToString() == barcode && string.IsNullOrEmpty(barcode) == false)
				{
					result = new SampleInformation()
					{
						Age = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_Age].ToString(),
						Barcode = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_Barcode].ToString(),
						Birthday = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_Birthday].ToString(),
						DateTime = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_DateTime].ToString(),
						Name = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_Name].ToString(),
						Reagent = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_Item].ToString(),
						SampleId = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_SampleID].ToString(),
						Sex = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_Sex].ToString(),
						Type = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_SampleType].ToString(),
						IsEmergency = i % 2 == 0 ? true : false
					};
				}
			}
			return result;
		}

		public string EntityToXmlString(object entity, bool isFormat = false)
		{
			return Sias.Core.SXmlConverter.ToXMLString(entity, isFormat);
		}

		public object XmlStringToEntity(string xmlString)
		{
			return Sias.Core.SXmlConverter.CreateFromXMLString(xmlString);
		}

		public bool XmlStringToEntity2(object obj, string xmlString)
		{
			return Sias.Core.SXmlConverter.FromXMLString(obj, xmlString);
		}

		public void SetSampleRackState(SampleRackStateArgs args)
		{
			var sampleRackIndex = (int)args.SampleRackIndex;
			this.RollBackSampleRacksState(false, args.SampleRackIndex);
			this.FourSampleRackDescriptions[sampleRackIndex].SampleRackState = args.SampleRackState;
			this.CurrentSampleInformations = this.FourSampleInformations[sampleRackIndex];
			this.RaisePropertyChanged(nameof(this.CurrentSampleInformations));
		}

		private void RollBackSampleRacksState(bool isRollBackCurrentIndex, SampleRackIndex sampleRack)
		{
			var currentIndex = (int)sampleRack;
			if (isRollBackCurrentIndex) this.FourSampleRackDescriptions[currentIndex].RollbackState();
			else
			{
				for (int i = 0; i < this.FourSampleRackDescriptions.Count; i++)
				{
					if (currentIndex != i) this.FourSampleRackDescriptions[i].RollbackState();
				}
			}
		}

		public void GetLisTableFromFile()
		{
			try
			{
				string dateTime = string.Empty;

#if DEBUG
				dateTime = "20170524";
#else
				dateTime=DateTime.Now.ToString(Properties.Resources.LisFileNameFormat);
#endif
				var lisFilesPath = string.Format
				(
					ConfigurationManager.AppSettings[Properties.Resources.LisFilesPath].ToString(),
					/*Directory.GetCurrentDirectory()*/
					Environment.CurrentDirectory,
					dateTime
				);

				if (File.Exists(lisFilesPath))
				{
					this.lisInformationTable = XmlOperation.ReadXmlFile(lisFilesPath).Tables[2];
					if (this.lisInformationTable == null) { /*General.ShowAdministratorsView(); */return; }
				}
				else { /*General.ShowAdministratorsView();*/ return; }
			}
			catch
			{
				General.PopupWindow(PopupType.OneButton, General.FindStringResource(Properties.Resources.PopupWindow_Message1), null);
			}
		}

		public void TestMethod(SampleRackIndex sampleColumn)
		{
			var sampleInformationsColumns = new ObservableCollection<SampleInformation>();

			if (this.lisInformationTable != null)
			{
				for (int i = 0; i < this.lisInformationTable.Rows.Count; i++)
				{
					var sampleInformation = new SampleInformation()
					{
						Age = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_Age].ToString(),
						Barcode = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_Barcode].ToString(),
						Birthday = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_Birthday].ToString(),
						DateTime = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_DateTime].ToString(),
						HoleName = this.GetHoleNameByNumber(i + 1 + (20 * (int)sampleColumn)),
						Name = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_Name].ToString(),
						Reagent = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_Item].ToString(),
						SampleId = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_SampleID].ToString(),
						Sex = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_Sex].ToString(),
						Type = this.lisInformationTable.Rows[i][Properties.Resources.LisInfo_SampleType].ToString(),
						IsEmergency = i % 2 == 0 ? true : false
					};
					sampleInformationsColumns.Add(sampleInformation);
				}
				this.CurrentSampleInformations = sampleInformationsColumns;
				this.FourSampleInformations[(int)sampleColumn] = sampleInformationsColumns;
				this.RaisePropertyChanged(nameof(this.CurrentSampleInformations));
			}
		}
	}


	public class SampleRackStateArgs : EventArgs
	{
		public SampleRackIndex SampleRackIndex { get; set; } = 0;
		public SampleRackState SampleRackState { get; set; } = SampleRackState.NotSample;

		public SampleRackStateArgs(SampleRackIndex sampleRackIndex, SampleRackState sampleRackState)
		{
			this.SampleRackIndex = sampleRackIndex;
			this.SampleRackState = sampleRackState;
		}
	}
}
