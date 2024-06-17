using System;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.SystemMapNotification;

public class SystemMapNotificationBaseVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveCommand ShowNotificationCommand = new ReactiveCommand();

	public readonly ReactiveProperty<bool> IsShowUp = new ReactiveProperty<bool>(initialValue: false);

	protected override void DisposeImplementation()
	{
	}

	public void ForceClose()
	{
		IsShowUp.Value = false;
	}
}
