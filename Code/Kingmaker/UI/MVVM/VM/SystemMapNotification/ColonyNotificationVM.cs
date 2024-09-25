using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.SystemMapNotification;

public class ColonyNotificationVM : SystemMapNotificationBaseVM, IColonyNotificationUIHandler, ISubscriber
{
	public readonly ReactiveProperty<ColonyNotificationData> Data = new ReactiveProperty<ColonyNotificationData>();

	private ColonyNotificationType m_Type;

	public ColonyNotificationVM()
	{
		AddDisposable(EventBus.Subscribe(this));
	}

	public void HandleColonyNotification(string colonyName, ColonyNotificationType type)
	{
		if (!Game.Instance.Player.ColoniesState.ForbidColonization)
		{
			ColonyNotificationData value = default(ColonyNotificationData);
			value.Name = colonyName;
			value.Type = type;
			Data.Value = value;
			ShowNotificationCommand.Execute();
		}
	}
}
