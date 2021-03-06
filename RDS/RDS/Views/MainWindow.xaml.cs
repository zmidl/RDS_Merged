﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RDS.ViewModels.Common;
using System.Runtime.InteropServices;
using RdCore;
using Sias.Core;

namespace RDS.Views
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{

		bool aa;

		private object CurrentContent;

		private MainView mainView;

		public static event EventHandler<GlobalNotifyArgs> GlobalNotify;

		public virtual void OnGlobalNotify(GlobalNotifyArgs myArgs)
		{
			MainWindow.GlobalNotify?.Invoke(null, myArgs);
		}

		public MainWindow()
		{
			InitializeComponent();
			General.InitializeMainWindow(this);
			this.LoadLanguage();
			this.mainView = new MainView();
			this.CurrentContent = this.Content;
			this.DisplayTwoButton();

			SingletonWindow.Process();
		}

		private void DisplayTwoButton()
		{
			Task task2 = new Task(() =>
			{
				try
				{

				}
				catch (Exception ex)
				{
					var ee = ex.Message;
				}
				//for (int i = 12; i > 0; i--)
				//{
				//System.Threading.Thread.Sleep(3000);
				//	this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { this.TextBlock_count.Text = i.ToString(); }));
				//}
			});
			task2.ContinueWith(t =>
			{
				this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => { this.Content = this.mainView; }));
			});

			task2.Start();


			//General.WriteConfiguration(Properties.Resources.Language, "Chinese"); 
		}

		public ResourceDictionary ResourceDictionary { get; private set; }
		private void LoadLanguage()
		{
			string language = string.Empty;
			var languageItems = General.ReadConfiguration(Properties.Resources.LanguageInformation).Split(Properties.Resources.Separator2.ToCharArray()[0]);
			for (int i = 0; i < languageItems.Length; i++)
			{
				var currentItem = languageItems[i].Split(Properties.Resources.Separator3.ToCharArray()[0]);
				if (currentItem[2] == Properties.Resources.One) language = currentItem[1];
			}
			var languagePath = string.Format(General.ReadConfiguration(Properties.Resources.LanguagePath), language);
			this.ResourceDictionary = Application.LoadComponent(new Uri(languagePath, UriKind.Relative)) as ResourceDictionary;
			if (this.ResourceDictionary != null)
			{
				if (this.Resources.MergedDictionaries.Count > 0)
				{
					this.Resources.MergedDictionaries.Clear();
				}
				this.Resources.MergedDictionaries.Add(this.ResourceDictionary);
			}
		}

		private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.Content = this.CurrentContent;
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl))
			{
				if (Keyboard.IsKeyDown(Key.L))
				{
					this.aa = !this.aa;

					ResourceDictionary langRd = null;
					try
					{
						if (aa) langRd = Application.LoadComponent(new Uri(Properties.Resources.English, UriKind.Relative)) as ResourceDictionary;
						else langRd = Application.LoadComponent(new Uri(Properties.Resources.Chinese, UriKind.Relative)) as ResourceDictionary;
					}
					catch
					{

					}

					if (langRd != null)
					{
						if (this.Resources.MergedDictionaries.Count > 0)
						{
							this.Resources.MergedDictionaries.Clear();
						}
						this.Resources.MergedDictionaries.Add(langRd);
					}
				}
				else if (Keyboard.IsKeyDown(Key.S))
				{
					if (Keyboard.IsKeyDown(Key.NumPad1)) this.OnGlobalNotify(new GlobalNotifyArgs($"SampleState1"));
					else if (Keyboard.IsKeyDown(Key.NumPad2)) this.OnGlobalNotify(new GlobalNotifyArgs($"SampleState2"));
					else if (Keyboard.IsKeyDown(Key.NumPad3)) this.OnGlobalNotify(new GlobalNotifyArgs($"SampleState3"));
					else if (Keyboard.IsKeyDown(Key.NumPad4)) this.OnGlobalNotify(new GlobalNotifyArgs($"SampleState4"));

					else if (Keyboard.IsKeyDown(Key.D1)) this.OnGlobalNotify(new GlobalNotifyArgs($"Sample1"));
					else if (Keyboard.IsKeyDown(Key.D2)) this.OnGlobalNotify(new GlobalNotifyArgs($"Sample2"));
					else if (Keyboard.IsKeyDown(Key.D3)) this.OnGlobalNotify(new GlobalNotifyArgs($"Sample3"));
					else if (Keyboard.IsKeyDown(Key.D4)) this.OnGlobalNotify(new GlobalNotifyArgs($"Sample4"));

				}
				else if (Keyboard.IsKeyDown(Key.M))
				{
					if (Keyboard.IsKeyDown(Key.NumPad1))
					{
						this.OnGlobalNotify(new GlobalNotifyArgs($"MixtureState1"));
					}
					else if (Keyboard.IsKeyDown(Key.NumPad2))
					{
						this.OnGlobalNotify(new GlobalNotifyArgs($"MixtureState2"));
					}
				}
				else if (Keyboard.IsKeyDown(Key.R))
				{
					if (Keyboard.IsKeyDown(Key.R) && Keyboard.IsKeyDown(Key.D)) {; }

					else if (Keyboard.IsKeyDown(Key.NumPad1)) this.OnGlobalNotify(new GlobalNotifyArgs($"Enzyme+"));

					else if (Keyboard.IsKeyDown(Key.NumPad2)) this.OnGlobalNotify(new GlobalNotifyArgs($"Enzyme-"));
				}
			}
		}

	}

	public class GlobalNotifyArgs : EventArgs
	{
		public string Index { get; set; }

		public GlobalNotifyArgs(string index)
		{
			this.Index = index;
		}
	}
}
