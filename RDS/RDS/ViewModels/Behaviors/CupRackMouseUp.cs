using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace RDS.ViewModels.Behaviors
{
	public class CupRackMouseUp : Behavior<UIElement>
	{
		public int RackIndex
		{
			get { return (int)GetValue(RackIndexProperty); }
			set { SetValue(RackIndexProperty, value); }
		}
		public static readonly DependencyProperty RackIndexProperty =
			DependencyProperty.Register(nameof(RackIndex), typeof(int), typeof(CupRackMouseUp), new PropertyMetadata(0));

		public object ViewModel
		{
			get { return GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}
		public static readonly DependencyProperty ViewModelProperty =
		   DependencyProperty.Register(nameof(ViewModel), typeof(object), typeof(CupRackMouseUp), new PropertyMetadata(null));

		private void AssociatedObject_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var viewModel = (MonitorViewModel)this.ViewModel;
			viewModel.ShowStripView();
			//throw new NotImplementedException();
		}

		protected override void OnAttached()
		{
			base.OnAttached();
			this.AssociatedObject.MouseUp += AssociatedObject_MouseUp;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();
			this.AssociatedObject.MouseUp -= AssociatedObject_MouseUp;
		}
	}
}
