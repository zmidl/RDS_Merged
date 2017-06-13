using RDS.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDS.Models;

namespace RDS.ViewModels
{
	public class PopupWindowViewModel : ViewModel
	{
		
	

		private string popupTitle = string.Empty;
		public string PopupTitle
		{
			get { return popupTitle; }
			set
			{
				popupTitle = value;
				this.RaisePropertyChanged(nameof(PopupTitle));
			}
		}

		private string message = string.Empty;
		public string Message
		{
			get { return message; }
			set
			{
				message = value;
				this.RaisePropertyChanged(nameof(Message));
			}
		}

		private PopupType popupType;
		public PopupType PopupType
		{
			get { return popupType; }
			set
			{
				popupType = value;
				this.RaisePropertyChanged(nameof(PopupType));
				this.PopupTypeIndex = (int)value;
				this.RaisePropertyChanged(nameof(this.PopupTypeIndex));
			}
		}

		private readonly string[] popupWindowTitle;

		public int PopupTypeIndex { get; set; }

		private Action[] Actions;

		public RelayCommand Command { get; private set; }

		public PopupWindowViewModel()
		{
			this.Command = new RelayCommand(this.ExecuteCommand);

			this.popupWindowTitle = new string[7]
			{
				Properties.Resources.PopupWindow_Title_MessageBox,
				Properties.Resources.PopupWindow_Title_Administrators,
				Properties.Resources.PopupWindow_Title_Administrators,
				Properties.Resources.PopupWindow_Title_Wait,
				Properties.Resources.PopupWindow_Title_Information,
				Properties.Resources.PopupWindow_Title_MessageBox,
				Properties.Resources.PopupWindow_Title_MessageBox
			};
		}

		public enum ViewChangedOption
		{
			ExitView = 0,
			EnterAdministratorsView = 1
		}

		public class PopupWindowViewChangedArgs : EventArgs
		{
			public ViewChangedOption Option { get; set; }
			public object Value { get; set; }

			public PopupWindowViewChangedArgs(ViewChangedOption option, object value)
			{
				this.Option = option;
				this.Value = value;
			}
		}

		private void ExecuteCommand(object actionIndex)
		{
			int index = int.Parse(actionIndex.ToString());
			this.OnViewChanged(new PopupWindowViewChangedArgs(ViewChangedOption.ExitView, null));
			if (this.Actions[index] == null) this.Actions[index] = new Action(() => {; });
			this.Actions[index]();
		}

		public void ValidateAdministrators()
		{
			this.OnViewChanged(new PopupWindowViewChangedArgs(ViewChangedOption.EnterAdministratorsView, null));
		}
	
		public void PopupWindow(PopupType popupType, string message, Action[] actions)
		{
			this.PopupType = popupType;
			this.Message = message;
			this.Actions = actions;
			this.PopupTitle = General.FindStringResource(this.popupWindowTitle[(int)popupType]);
			//if (popupType == PopupType.BackstageLogin)
			//{
			//	this.Actions = new Action[3];
			//	this.Actions[0] = new Action(this.ValidateAdministrators);
			//}
		}
	}

	
}
