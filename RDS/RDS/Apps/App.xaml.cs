using System;
using System.Windows;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using RdCore;
using RdEntity;

namespace RDS.Apps
{
	/// <summary>
	/// App.xaml 的交互逻辑
	/// </summary>
	public partial class App : Application
    {
		public static RdData GlobalData=new RdData();

		//System.Threading.Mutex mutex;
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			this.StartupUri = new Uri(RDS.Properties.Resources.StartupUri, UriKind.Relative);
			
		}
	}
}
