using System.Collections;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace RDSCL
{
	/// <summary>
	/// TipRack.xaml 的交互逻辑
	/// </summary>
	public partial class TipRack : UserControl
	{
		public object DataSource
		{
			get { return GetValue(DataSourceProperty); }
			set { SetValue(DataSourceProperty, value); }
		}
		public static readonly DependencyProperty DataSourceProperty =
			DependencyProperty.Register(nameof(DataSource), typeof(object), typeof(TipRack), new PropertyMetadata(null));


		public bool IsTwinkle
		{
			get { return (bool)GetValue(IsTwinkleProperty); }
			set { SetValue(IsTwinkleProperty, value); }
		}
		public static readonly DependencyProperty IsTwinkleProperty =
			DependencyProperty.Register(nameof(IsTwinkle), typeof(bool), typeof(TipRack), new PropertyMetadata(false, new PropertyChangedCallback(Callback_IsTwinkle)));

		public bool? CurrentState
		{
			get { return (bool?)GetValue(CurrentStateProperty); }
			set { SetValue(CurrentStateProperty, value); }
		}
		public static readonly DependencyProperty CurrentStateProperty =
			DependencyProperty.Register(nameof(CurrentState), typeof(bool?), typeof(TipRack), new PropertyMetadata(null, new PropertyChangedCallback(Callback_CurrentState)));

		private static void Callback_CurrentState(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
			{
				var tipRack = d as TipRack;
				var currentState = (bool)e.NewValue;
				var frame = tipRack.Template.FindName("Rectangle_Frame", tipRack) as System.Windows.Shapes.Rectangle;
				var blue = tipRack.FindResource("BlueColor") as SolidColorBrush;
				var wathetColor3 = tipRack.FindResource("WathetColor3") as SolidColorBrush;
				var body = tipRack.Template.FindName("StackPanel_Body", tipRack) as Panel;
				switch (currentState)
				{
					case true:
					{
						frame.Fill = blue;
						frame.StrokeThickness = 0d;
						body.Visibility = Visibility.Visible;
						break;
					}
					case false:
					{
						frame.StrokeThickness = 1d;
						frame.Stroke = blue;
						frame.StrokeDashArray = new DoubleCollection() { 5 };

						frame.Fill = wathetColor3;
						body.Visibility = Visibility.Hidden;
						break;
					}
					default: { break; }
				}
			}
		}

		private static void Callback_IsTwinkle(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tipRack = (TipRack)d;

			var storyboard = tipRack.Resources[Properties.Resources.TwinkleAnimation] as Storyboard;

			var twinkleModule = (FrameworkElement)tipRack;

			if ((bool)e.NewValue == true)
			{
				storyboard.RepeatBehavior = new RepeatBehavior(1);

				storyboard.Completed += (sender, eventArgs) =>
				{
					if (tipRack.IsTwinkle) storyboard.Begin(twinkleModule);
				};
				storyboard.Begin(twinkleModule);
			}
		}

		public TipType TipType
		{
			get { return (TipType)GetValue(TipTypeProperty); }
			set { SetValue(TipTypeProperty, value); }
		}
		public static readonly DependencyProperty TipTypeProperty =
			DependencyProperty.Register(nameof(TipType), typeof(TipType), typeof(TipRack), new PropertyMetadata(TipType._noneStyle,new PropertyChangedCallback(Callback_TipType)));

		private static void Callback_TipType(DependencyObject d,DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue != null)
			{
				var newValue = (TipType)e.NewValue;
				var tipRack = d as TipRack;

				Style style = default(Style);
				if (newValue == TipType._1000uLStyle)
				{
					style = tipRack.FindResource("_1000uLStyle") as Style;
					var margin = (tipRack.Template.FindName("StackPanel_0", tipRack) as StackPanel);
					if(margin!=null) margin.Margin = new Thickness(12, 12, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_1", tipRack) as StackPanel);
					if (margin != null) margin.Margin = new Thickness(12, 2, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_2", tipRack) as StackPanel);
					if (margin != null) margin.Margin = new Thickness(12, 2, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_3", tipRack) as StackPanel);
					if (margin != null) margin.Margin = new Thickness(12, 2, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_4", tipRack) as StackPanel);
					if (margin != null) margin.Margin = new Thickness(12, 2, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_5", tipRack) as StackPanel);
					if (margin != null) margin.Margin = new Thickness(12, 2, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_6", tipRack) as StackPanel);
					if (margin != null) margin.Margin = new Thickness(12, 2, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_7", tipRack) as StackPanel);
					if (margin != null) margin.Margin = new Thickness(12, 2, 0, 0);
				}
				else
				{
					style = tipRack.FindResource("_300uLStyle") as Style;

					var margin = (tipRack.Template.FindName("StackPanel_0", tipRack) as StackPanel);
					if(margin!=null) margin.Margin = new Thickness(5, 6, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_1", tipRack) as StackPanel);
					if(margin!=null) margin.Margin = new Thickness(5, 2, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_2", tipRack) as StackPanel);
					if (margin != null) margin.Margin = new Thickness(5, 2, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_3", tipRack) as StackPanel);
					if (margin != null) margin.Margin = new Thickness(5, 2, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_4", tipRack) as StackPanel);
					if (margin != null) margin.Margin = new Thickness(5, 2, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_5", tipRack) as StackPanel);
					if (margin != null) margin.Margin = new Thickness(5, 2, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_6", tipRack) as StackPanel);
					if (margin != null) margin.Margin = new Thickness(5, 2, 0, 0);

					margin = (tipRack.Template.FindName("StackPanel_7", tipRack) as StackPanel);
					if (margin != null) margin.Margin = new Thickness(5, 2, 0, 0);
				}

				for (int i = 0; i < 96; i++)
				{
					var ellipse = tipRack.Template.FindName("Ellipse_" + i.ToString(), tipRack) as Ellipse;
					if (ellipse != null) ellipse.Style = style;

				}
			}
		}

		public TipRack()
		{
			InitializeComponent();
		}
	}

	public enum TipType
	{
		_noneStyle=0,
		_1000uLStyle = 1,
		_300uLStyle = 2
	}

}
