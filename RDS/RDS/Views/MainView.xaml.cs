using RDS.ViewModels;
using RDS.ViewModels.Common;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace RDS.Views
{
	/// <summary>
	/// MainView.xaml 的交互逻辑
	/// </summary>
	public partial class MainView : UserControl
	{

		private TaskView taskView = new TaskView();

		private HistroyView histroyView = new HistroyView();

		private HelpView helpView = new HelpView();

		private SetupView setupView = new SetupView();

		private object PreviousContent;

		public MainViewModel ViewModel { get { return this.DataContext as MainViewModel; } }

		public MainView()
		{
			InitializeComponent();

			this.DataContext = new MainViewModel();

			this.ContentControl_CurrentContent.Content = this.taskView;

			this.PreviousContent = this.ContentControl_CurrentContent.Content;

			this.ViewModel.ViewChanged += ViewModel_ViewChanged;

			//PopupWindow popupWindow = new PopupWindow();

			//popupWindow.DataContext = this.ViewModel.PopupWindowViewModel;

			//General.InitializePopupWindow(popupWindow);
		}

		private void ViewModel_ViewChanged(object sender, object e)
		{
			switch ((MainViewModel.ViewChangedOption)e)
			{
				case MainViewModel.ViewChangedOption.TaskView: { this.ContentControl_CurrentContent.Content = this.taskView; break; }
				case MainViewModel.ViewChangedOption.HistroyView: { this.ContentControl_CurrentContent.Content = this.histroyView; break; }
				case MainViewModel.ViewChangedOption.HelpView: { this.ContentControl_CurrentContent.Content = this.helpView; break; }
				case MainViewModel.ViewChangedOption.ExitApp: { Application.Current.Shutdown();/*Environment.Exit(0);*/break; }
				case MainViewModel.ViewChangedOption.SetupView: { this.ContentControl_CurrentContent.Content = this.setupView; break; }
				default: break;
			}
		}

		private void Button_Minimize_Click(object sender, RoutedEventArgs e)
		{
			TestView testView = new TestView();
			ContentControl_CurrentContent.Content = testView;
		}

		private void Button_Information_Click(object sender, RoutedEventArgs e)
		{
			General.PopupWindow(PopupType.OneButton, string.Join("-", General.UsedReagents.ToArray()), null);
		}
    }
}
