using System.Windows;
using System.Windows.Controls;

namespace RDSCL
{
	/// <summary>
	/// TipRack_300uL.xaml 的交互逻辑
	/// </summary>
	public partial class TipRack_300uL : UserControl
	{
		public object DataSource
		{
			get { return GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}
		public static readonly DependencyProperty ItemsSourceProperty =
			DependencyProperty.Register(nameof(DataSource), typeof(object), typeof(TipRack_300uL), new PropertyMetadata(null));


		public TipRack_300uL()
		{
			InitializeComponent();
		}
	}
}
