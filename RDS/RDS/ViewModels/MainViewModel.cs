using RDS.ViewModels.Common;
using System;

namespace RDS.ViewModels
{
	public class MainViewModel : ViewModel
	{
		private bool isTask;
		public bool IsTask
		{
			get { return isTask; }
			set
			{
				if (isTask == false)
				{
					isTask = value;
					this.RaisePropertyChanged(nameof(IsTask));
					this.isHistroy = false;
					this.isHelp = false;
					this.RaisePropertyChanged(nameof(this.IsHistroy));
					this.RaisePropertyChanged(nameof(this.IsHelp));
					this.ShowTaskView();
				}
			}
		}

		private bool isHistroy;
		public bool IsHistroy
		{
			get { return isHistroy; }
			set
			{
				if (isHistroy == false)
				{
					isHistroy = value;
					this.RaisePropertyChanged(nameof(IsHistroy));
					this.isTask = false;
					this.isHelp = false;
					this.RaisePropertyChanged(nameof(this.IsTask));
					this.RaisePropertyChanged(nameof(this.IsHelp));
					this.ShowHistroyView();
				}
			}
		}

		private bool isHelp;
		public bool IsHelp
		{
			get { return isHelp; }
			set
			{
				if (isHelp == false)
				{
					isHelp = value;
					this.RaisePropertyChanged(nameof(IsHelp));
					this.isTask = false;
					this.isHistroy = false;
					this.RaisePropertyChanged(nameof(this.IsTask));
					this.RaisePropertyChanged(nameof(this.IsHistroy));
					this.ShowHelpView();
				}
			}
		}

		public PopupWindowViewModel PopupWindowViewModel { get; set; } = new PopupWindowViewModel();

		public RelayCommand ExitApp { get; private set; }
	
		public RelayCommand ShowAdministratorsLogin { get; private set; }
		public RelayCommand ShowAdministratorsView { get; private set; }
		public RelayCommand ShowMessageView { get; private set; }
		public RelayCommand ShowInformation { get; private set; }
		public RelayCommand ShowCricleProgress { get; private set; }


		public enum ViewChangedOption
		{
			TaskView = 0,
			HistroyView = 1,
			HelpView = 2,
			SetupView = 3,
			Minisize = 4,
			ExitApp = 5
		}

		public class MainViewChangedArgs : EventArgs
		{
			public ViewChangedOption Option { get; set; }
			public object Value { get; set; }

			public MainViewChangedArgs(ViewChangedOption option, object value)
			{
				this.Option = option;
				this.Value = value;
			}
		}
		public MainViewModel()
		{
			this.ShowInformation = new RelayCommand(() => General.PopupWindow(PopupType.OneButton,General.FindStringResource(Properties.Resources.PopupWindow_Title_Information),null));
			//this.ShowAdministratorsLogin = new RelayCommand(()=>General.PopupWindow(PopupType.BackstageLogin,string.Empty,null));
			this.ShowAdministratorsLogin = new RelayCommand(this.ExecuteShowSetupView);

			this.ExitApp = new RelayCommand(this.ExecuteExitApp);
			this.IsTask = true;
		}

		private void ShowTaskView()
		{
			this.OnViewChanged(ViewChangedOption.TaskView);
		}

		private void ShowHistroyView()
		{
			this.OnViewChanged(ViewChangedOption.HistroyView);
		}

		private void ShowHelpView()
		{
			this.OnViewChanged(ViewChangedOption.HelpView);
		}

		private void ExecuteShowSetupView()
		{
			this.IsTask = false;
			this.IsHistroy = false;
			this.IsHelp = false;
			this.OnViewChanged(ViewChangedOption.SetupView);
		}

		private void ExecuteExitApp()
		{
			this.OnViewChanged(ViewChangedOption.ExitApp);
		}
	}
}
