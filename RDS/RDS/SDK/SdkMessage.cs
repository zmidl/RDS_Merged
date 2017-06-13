using RDS.ViewModels.Common;
using Sias.ReSaTrax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RDS.SDK
{
	public class SdkMessage : IDialogHelper
	{
		public class SdkMessageArgs : EventArgs
		{
			public PopupType PopupType { get; set; }
			public string Title { get; set; }
			public string Message { get; set; }
			public Action[] Actions { get; set; }
			public Window Window { get; set; }
		}

		private bool canRetry = true;

		public SdkMessageArgs Args { get; set; }

		public SdkMessage(PopupType popupType, string title, string message, Window window/*, Action[] actions*/)
		{
			var actions = new Action[2];
			if (popupType == PopupType.OneButton) { actions[0] = new Action(this.Abort); actions[1] = new Action(() => {; }); }
			else if (popupType == PopupType.TwoButton) { actions[0] = new Action(this.Retry); actions[1] = new Action(this.Abort); }
			this.Args = new SdkMessageArgs() { PopupType = popupType, Title = title, Message = message, Actions = actions, Window = window };
		}

		public event EventHandler<SdkMessageArgs> EventHandler;

		public virtual void OnMyMessageBoxShow(SdkMessageArgs args)
		{
			this.EventHandler?.Invoke(this, args);
		}

		public virtual bool IsAborted { get; set; }

		public virtual bool IsRetryRequested { get; set; }

		public virtual void Done() { this.Args.Window.Close(); }

		public virtual void Failed()
		{
			this.canRetry = true;
			while (!((IDialogHelper)this).IsRetryRequested && !((IDialogHelper)this).IsAborted)
			{
				WpfApplication.DoEvents();
			}
		}

		public void ShowDialog(int slotIndex)
		{
			this.OnMyMessageBoxShow(this.Args);
		}

		private void Abort()
		{
			IsAborted = false;
			IsRetryRequested = true;
		}

		private void Retry()
		{
			if (this.canRetry)
			{
				IsAborted = false;
				IsRetryRequested = true;
				this.canRetry = false;
			}
		}
	}
}
