using RDS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RDS.Views
{
	/// <summary>
	/// SetupView.xaml 的交互逻辑
	/// </summary>
	public partial class SetupView : UserControl
	{
		public SetupViewModel ViewModel { get { return this.DataContext as SetupViewModel; } }
		public SetupView()
		{
			InitializeComponent();
			this.DataContext = new SetupViewModel();
			this.ViewModel.ViewChanged += ViewModel_ViewChanged;
			ViewModels.Common.General.FindSetupView(this);
		}

		private void ViewModel_ViewChanged(object sender, object e)
		{
			var args = (SetupViewModel.SetupViewChangedArgs)e;
			if (this.PasswordBox1.Password == args.Value.ToString()) this.ViewModel.ViewIndex = 1;
			//throw new NotImplementedException();
		}
	}
}
