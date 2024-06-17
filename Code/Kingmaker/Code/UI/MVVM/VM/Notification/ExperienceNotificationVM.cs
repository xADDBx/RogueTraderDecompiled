using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Notification;

public class ExperienceNotificationVM : NotificationVM, IExperienceNotificationUIHandler, ISubscriber
{
	public readonly ReactiveProperty<string> ShowExperienceAmount = new ReactiveProperty<string>(string.Empty);

	public ExperienceNotificationVM()
	{
		EventBus.Subscribe(this);
	}

	protected override void DisposeImplementation()
	{
		EventBus.Unsubscribe(this);
	}

	public void HandleExperienceNotification(int amount)
	{
		ShowExperienceAmount.Value = amount.ToString();
		ShowNotification();
	}
}
