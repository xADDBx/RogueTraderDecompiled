using System;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Notification;

public class NotificationVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveCommand ShowNotificationCommand = new ReactiveCommand();

	protected override void DisposeImplementation()
	{
	}

	protected void ShowNotification()
	{
		ShowNotificationCommand.Execute();
	}
}
