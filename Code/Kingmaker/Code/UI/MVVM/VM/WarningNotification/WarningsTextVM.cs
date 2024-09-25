using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.WarningNotification;

public class WarningsTextVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IWarningNotificationUIHandler, ISubscriber
{
	public readonly ReactiveProperty<string> ShowString = new ReactiveProperty<string>();

	public readonly ReactiveProperty<WarningNotificationFormat> ShowFormat = new ReactiveProperty<WarningNotificationFormat>(WarningNotificationFormat.Common);

	public bool ShowWithSound;

	public WarningsTextVM()
	{
		EventBus.Subscribe(this);
	}

	protected override void DisposeImplementation()
	{
		EventBus.Unsubscribe(this);
	}

	public void HandleWarning(WarningNotificationType warningType, bool addToLog, WarningNotificationFormat warningFormat, bool withSound = true)
	{
		ShowWithSound = withSound;
		ShowFormat.Value = warningFormat;
		ShowString.SetValueAndForceNotify(LocalizedTexts.Instance.WarningNotification.GetText(warningType));
	}

	public void HandleWarning(string str, bool addToLog, WarningNotificationFormat warningFormat, bool withSound = true)
	{
		ShowWithSound = withSound;
		ShowFormat.Value = warningFormat;
		ShowString.SetValueAndForceNotify(str);
	}
}
