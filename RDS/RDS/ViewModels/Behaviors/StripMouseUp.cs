using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace RDS.ViewModels.Behaviors
{
	public class StripMouseUp : Behavior<UIElement>
	{
		public int StripIndex
		{
			get { return (int)GetValue(StripProperty); }
			set { SetValue(StripProperty, value); }
		}
		public static readonly DependencyProperty StripProperty =
			DependencyProperty.Register(nameof(StripIndex), typeof(int), typeof(StripMouseUp), new PropertyMetadata(0));

		public object ViewModel
		{
			get { return GetValue(ViewModelProperty); }
			set { SetValue(ViewModelProperty, value); }
		}
		public static readonly DependencyProperty ViewModelProperty =
		   DependencyProperty.Register(nameof(ViewModel), typeof(object), typeof(StripMouseUp), new PropertyMetadata(null));

		private void AssociatedObject_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var viewModel = (StripViewModel)this.ViewModel;
			viewModel.SetStripState(this.StripIndex);
			viewModel.UpdateSelectedUsedCount(this.StripIndex);
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
