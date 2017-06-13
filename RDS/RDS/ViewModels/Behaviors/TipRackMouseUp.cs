using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace RDS.ViewModels.Behaviors
{
	public class TipRackMouseUp : Behavior<UIElement>
	{
		public int TipRackIndex
		{
			get { return (int)GetValue(TipRackIndexProperty); }
			set { SetValue(TipRackIndexProperty, value); }
		}
		public static readonly DependencyProperty TipRackIndexProperty =
			DependencyProperty.Register(nameof(TipRackIndex), typeof(int), typeof(TipRackMouseUp), new PropertyMetadata(0));

		public object ViewModel
		{
			get { return GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}
		public static readonly DependencyProperty ViewModelProperty =
		   DependencyProperty.Register(nameof(ViewModel), typeof(object), typeof(TipRackMouseUp), new PropertyMetadata(null));

		private void AssociatedObject_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var viewModel = (MonitorViewModel)this.ViewModel;
			viewModel.SetTipRackState(this.TipRackIndex);
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
