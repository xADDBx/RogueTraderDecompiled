using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.MVVM.VM.SystemMapNotification;

public class EncyclopediaNotificationVM : SystemMapNotificationBaseVM, IEncyclopediaNotificationUIHandler, ISubscriber
{
	public string EncyclopediaLink;

	public string EncyclopediaName;

	public EncyclopediaNotificationVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	public void HandleEncyclopediaNotification(string link, string encyclopediaName)
	{
		EncyclopediaLink = link;
		EncyclopediaName = encyclopediaName;
		ShowNotificationCommand.Execute();
	}
}
